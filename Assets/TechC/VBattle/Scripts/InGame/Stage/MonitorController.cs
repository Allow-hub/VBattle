using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Video;
using TechC.VBattle.Core.Extensions;

namespace TechC.VBattle.InGame.Stage
{
    /// <summary>
    /// モニター表示を制御する
    ///
    /// 表示の種類
    /// - 画像（Texture）
    /// - 動画（VideoClip を VideoPlayer で再生し、RenderTexture に出して貼る）
    /// - カメラ映像（Camera の出力を RenderTexture に出して貼る）
    ///
    /// 切り替えの動き
    /// - 10〜30秒の範囲で待ち時間をランダムに決める
    /// - 時間が来たら「画像/動画/カメラ」からランダムに1つを選び、モニターへ反映する
    ///
    /// 画面に反映する方法
    /// - モニターの Renderer の Material に対して
    ///   URPなら _BaseMap（texturePropertyName）へ SetTexture
    ///   無ければ _MainTex へ SetTexture
    /// </summary>
    public class MonitorController : MonoBehaviour
    {
        public enum DisplayMode
        {
            Texture,
            Video,
            CameraFeed,
        }

        [Header("Start")]
        [SerializeField] private bool autoPlayOnStart = true;
        [SerializeField] private bool startWithPlaylist = true;
        [SerializeField] private DisplayMode startSingleMode = DisplayMode.Texture;

        [Header("Target (monitor surface)")]
        [SerializeField] private Renderer screenRenderer;
        [SerializeField] private int materialIndex = 0;
        [SerializeField] private string texturePropertyName = "_BaseMap";

        [Header("Playlist Candidates")]
        [SerializeField] private bool enableTextures = true;
        [SerializeField] private bool enableVideos = true;
        [SerializeField] private bool enableCameraFeed = true;

        [Header("Texture Candidates")]
        [SerializeField] private List<Texture> textures = new();
        [SerializeField] private bool shuffleTextures = true;
        [SerializeField] private int currentTextureIndex = 0;

        [Header("Video Candidates")]
        [SerializeField] private List<VideoClip> videoClips = new();
        [SerializeField] private bool shuffleVideos = true;
        [SerializeField] private int currentVideoIndex = 0;

        [Header("Interval")]
        [SerializeField] private float minInterval = 10f;
        [SerializeField] private float maxInterval = 30f;
        [SerializeField] private bool avoidSameModeTwice = true;

        [Header("Video Output")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RenderTexture videoRenderTexture;
        [SerializeField] private bool loopVideo = true;

        [Header("Camera Feed Output")]
        [SerializeField] private UnityEngine.Camera sourceBattleCamera;
        [SerializeField] private UnityEngine.Camera monitorCamera;
        [SerializeField] private RenderTexture cameraRenderTexture;

        public DisplayMode CurrentMode => currentMode;
        private DisplayMode currentMode;
        private DisplayMode lastMode;

        private Material runtimeMaterial;
        private Coroutine playlistCoroutine;

        private void Awake()
        {
            // モニターのRenderer確定
            if (screenRenderer == null)
                screenRenderer = GetComponent<Renderer>();

            if (screenRenderer == null)
            {
                Debug.LogError("[MonitorController] screenRenderer is missing.");
                enabled = false;
                return;
            }

            // 操作対象Material確定（indexチェック）
            var mats = screenRenderer.materials;
            if (materialIndex < 0 || materialIndex >= mats.Length)
            {
                Debug.LogError($"[MonitorController] materialIndex out of range. materials={mats.Length}");
                enabled = false;
                return;
            }

            runtimeMaterial = mats[materialIndex];

            // 環境差で反映されないのを避けるため、materials を戻して確定させる
            screenRenderer.materials = mats;

            // VideoPlayer初期設定
            if (videoPlayer != null)
            {
                videoPlayer.playOnAwake = false;
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                videoPlayer.isLooping = loopVideo;
            }
        }

        private void Start()
        {
            if (!autoPlayOnStart) return;

            if (startWithPlaylist)
                StartPlaylist();
            else
                ShowSingle(startSingleMode);
        }

        private void OnDestroy()
        {
            StopPlaylist();
            StopVideo();
            DisableMonitorCameraOutput();
        }

        // 外から「自動切り替え」を開始
        public void StartPlaylist()
        {
            StopPlaylist();
            playlistCoroutine = StartCoroutine(PlaylistLoop());
        }

        // 外から「自動切り替え」を停止（最後に映ってたまま止まる）
        public void StopPlaylist()
        {
            if (playlistCoroutine != null)
            {
                StopCoroutine(playlistCoroutine);
                playlistCoroutine = null;
            }
        }

        // 外から「単発でこのモードを表示」したい時（自動切り替えは止める）
        public void ShowSingle(DisplayMode mode)
        {
            StopPlaylist();
            ApplyMode(mode);
        }

        private IEnumerator PlaylistLoop()
        {
            // 起動直後に1回目の表示を決める
            ApplyMode(PickNextMode());

            while (true)
            {
                float wait = GetNextIntervalSeconds();
                yield return new WaitForSeconds(wait);

                ApplyMode(PickNextMode());
            }
        }

        private float GetNextIntervalSeconds()
        {
            float min = Mathf.Max(0.1f, Mathf.Min(minInterval, maxInterval));
            float max = Mathf.Max(min, Mathf.Max(minInterval, maxInterval));
            return Random.Range(min, max);
        }

        private DisplayMode PickNextMode()
        {
            // 候補を作る（有効で、必要データが揃ってるものだけ）
            List<DisplayMode> candidates = new();

            if (enableTextures && textures != null && textures.Count > 0)
                candidates.Add(DisplayMode.Texture);

            if (enableVideos && videoPlayer != null && videoRenderTexture != null && videoClips != null && videoClips.Count > 0)
                candidates.Add(DisplayMode.Video);

            if (enableCameraFeed && cameraRenderTexture != null && (monitorCamera != null || sourceBattleCamera != null))
                candidates.Add(DisplayMode.CameraFeed);

            if (candidates.Count == 0)
            {
                // 何もできない時はとりあえずTexture扱いにしておく（ログを出す）
                Debug.LogWarning("[MonitorController] No valid candidates. Check textures/videoClips/camera settings.");
                return DisplayMode.Texture;
            }

            // 直前と同じモードを避けたい場合（候補が2つ以上あるときだけ意味がある）
            if (avoidSameModeTwice && candidates.Count >= 2)
            {
                candidates.Remove(lastMode);
                if (candidates.Count == 0)
                {
                    // まれに全部消えた場合は元に戻す
                    candidates.Add(lastMode);
                }
            }

            int idx = Random.Range(0, candidates.Count);
            return candidates[idx];
        }

        private void ApplyMode(DisplayMode mode)
        {
            // まず前の表示の副作用を止める（動画やカメラの出力が残ると混ざるため）
            StopVideo();
            DisableMonitorCameraOutput();

            lastMode = currentMode;
            currentMode = mode;

            switch (currentMode)
            {
                case DisplayMode.Texture:
                    ApplyTextureNext();
                    break;

                case DisplayMode.Video:
                    ApplyVideoNext();
                    break;

                case DisplayMode.CameraFeed:
                    ApplyCameraFeed();
                    break;
            }
        }

        private void ApplyTextureNext()
        {
            if (textures == null || textures.Count == 0)
            {
                Debug.LogWarning("[MonitorController] textures is empty.");
                return;
            }

            // 次の画像を決める（順番 or ランダム）
            currentTextureIndex = GetNextIndex(currentTextureIndex, textures.Count, shuffleTextures);
            SetBaseTexture(textures[currentTextureIndex]);
        }

        private void ApplyVideoNext()
        {
            if (videoPlayer == null || videoRenderTexture == null)
            {
                Debug.LogWarning("[MonitorController] videoPlayer or videoRenderTexture is missing.");
                return;
            }

            if (videoClips == null || videoClips.Count == 0)
            {
                Debug.LogWarning("[MonitorController] videoClips is empty.");
                return;
            }

            // 次の動画を決める（順番 or ランダム）
            currentVideoIndex = GetNextIndex(currentVideoIndex, videoClips.Count, shuffleVideos);
            var clip = videoClips[currentVideoIndex];

            // モニターには「動画の出力先(RenderTexture)」を貼る
            SetBaseTexture(videoRenderTexture);

            // VideoPlayerの出力も同じRenderTextureに向ける
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = videoRenderTexture;
            videoPlayer.isLooping = loopVideo;

            // どの動画を流すかセットして、Prepare→完了後にPlay
            videoPlayer.clip = clip;

            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.Prepare();
        }

        private void OnVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnVideoPrepared;
            vp.Play();
        }

        private void StopVideo()
        {
            if (videoPlayer == null) return;

            videoPlayer.prepareCompleted -= OnVideoPrepared;
            if (videoPlayer.isPlaying) videoPlayer.Stop();

            // targetTexture を null に戻すかは好みだが、
            // 他所でVideoPlayerを使い回す可能性があるのでリセットしておく
            videoPlayer.targetTexture = null;
        }

        private void ApplyCameraFeed()
        {
            if (cameraRenderTexture == null)
            {
                Debug.LogWarning("[MonitorController] cameraRenderTexture is missing.");
                return;
            }

            // モニターには「カメラ出力(RenderTexture)」を貼る
            SetBaseTexture(cameraRenderTexture);

            // カメラが cameraRenderTexture に描画するようにする
            EnableMonitorCameraOutput();
        }

        private void EnableMonitorCameraOutput()
        {
            // monitorCamera を使う方式が安全（ゲーム画面のMainCameraを潰しにくい）
            if (monitorCamera != null)
            {
                if (sourceBattleCamera != null)
                {
                    // 戦闘カメラと同じ見た目にする（位置追従まで必要なら別途同期する）
                    monitorCamera.CopyFrom(sourceBattleCamera);
                }

                monitorCamera.targetTexture = cameraRenderTexture;
                monitorCamera.enabled = true;
                return;
            }

            // monitorCamera が無い場合は戦闘カメラへ直接 targetTexture を入れる（簡易）
            if (sourceBattleCamera == null)
            {
                Debug.LogWarning("[MonitorController] sourceBattleCamera is missing.");
                return;
            }

            sourceBattleCamera.targetTexture = cameraRenderTexture;
        }

        private void DisableMonitorCameraOutput()
        {
            if (monitorCamera != null)
            {
                monitorCamera.targetTexture = null;
                monitorCamera.enabled = false;
            }

            if (sourceBattleCamera != null && sourceBattleCamera.targetTexture == cameraRenderTexture)
            {
                sourceBattleCamera.targetTexture = null;
            }
        }

        private static int GetNextIndex(int current, int count, bool shuffle)
        {
            if (count <= 1) return 0;

            if (!shuffle)
                return (current + 1) % count;

            int next;
            do next = Random.Range(0, count);
            while (next == current);

            return next;
        }

        private void SetBaseTexture(Texture tex)
        {
            // ここが「モニターの見た目」を変える本体
            // URPなら _BaseMap、無ければ _MainTex
            if (runtimeMaterial == null) return;

            if (runtimeMaterial.HasProperty(texturePropertyName))
            {
                runtimeMaterial.SetTexture(texturePropertyName, tex);
                return;
            }

            if (runtimeMaterial.HasProperty("_MainTex"))
            {
                runtimeMaterial.SetTexture("_MainTex", tex);
                return;
            }

            Debug.LogWarning($"[MonitorController] Material has no property '{texturePropertyName}' nor '_MainTex'.");
        }
    }
}