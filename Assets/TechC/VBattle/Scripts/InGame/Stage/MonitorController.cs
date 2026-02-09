using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Video;

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
    /// - MaterialPropertyBlock を使って Renderer に直接テクスチャを渡す
    ///   → マテリアルを複製しないため軽量＆バッチング維持
    ///   URPなら _BaseMap、無ければ _MainTex へ SetTexture
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

        private Coroutine playlistCoroutine;

        // MaterialPropertyBlock（マテリアルを複製せず見た目だけ変更する）
        private MaterialPropertyBlock propertyBlock;
        private int texturePropertyID;
        private int mainTexID;

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

            // MaterialPropertyBlock初期化
            propertyBlock = new MaterialPropertyBlock();
            texturePropertyID = Shader.PropertyToID(texturePropertyName);
            mainTexID = Shader.PropertyToID("_MainTex");

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

            if (enableTextures && textures.Count > 0)
                candidates.Add(DisplayMode.Texture);

            if (enableVideos && videoPlayer && videoRenderTexture && videoClips.Count > 0)
                candidates.Add(DisplayMode.Video);

            if (enableCameraFeed && cameraRenderTexture && (monitorCamera || sourceBattleCamera))
                candidates.Add(DisplayMode.CameraFeed);

            if (candidates.Count == 0)
            {
                Debug.LogWarning("[MonitorController] No valid candidates.");
                return DisplayMode.Texture;
            }

            // 直前と同じモードを避けたい場合
            if (avoidSameModeTwice && candidates.Count >= 2)
            {
                candidates.Remove(lastMode);
                if (candidates.Count == 0)
                    candidates.Add(lastMode);
            }

            return candidates[Random.Range(0, candidates.Count)];
        }

        private void ApplyMode(DisplayMode mode)
        {
            // 前の表示を停止
            StopVideo();
            DisableMonitorCameraOutput();

            lastMode = currentMode;
            currentMode = mode;

            switch (mode)
            {
                case DisplayMode.Texture: ApplyTextureNext(); break;
                case DisplayMode.Video: ApplyVideoNext(); break;
                case DisplayMode.CameraFeed: ApplyCameraFeed(); break;
            }
        }

        private void ApplyTextureNext()
        {
            if (textures.Count == 0) return;

            currentTextureIndex = GetNextIndex(currentTextureIndex, textures.Count, shuffleTextures);
            SetBaseTexture(textures[currentTextureIndex]);
        }

        private void ApplyVideoNext()
        {
            if (!videoPlayer || !videoRenderTexture || videoClips.Count == 0) return;

            currentVideoIndex = GetNextIndex(currentVideoIndex, videoClips.Count, shuffleVideos);

            // モニターには動画出力先を貼る
            SetBaseTexture(videoRenderTexture);

            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = videoRenderTexture;
            videoPlayer.clip = videoClips[currentVideoIndex];
            videoPlayer.Prepare();

            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }

        private void OnVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnVideoPrepared;
            vp.Play();
        }

        private void StopVideo()
        {
            if (!videoPlayer) return;

            videoPlayer.prepareCompleted -= OnVideoPrepared;
            if (videoPlayer.isPlaying) videoPlayer.Stop();
            videoPlayer.targetTexture = null;
        }

        private void ApplyCameraFeed()
        {
            // モニターにはカメラ出力を貼る
            SetBaseTexture(cameraRenderTexture);
            EnableMonitorCameraOutput();
        }

        private void EnableMonitorCameraOutput()
        {
            if (monitorCamera)
            {
                if (sourceBattleCamera)
                    monitorCamera.CopyFrom(sourceBattleCamera);

                monitorCamera.targetTexture = cameraRenderTexture;
                monitorCamera.enabled = true;
                return;
            }

            if (sourceBattleCamera)
                sourceBattleCamera.targetTexture = cameraRenderTexture;
        }

        private void DisableMonitorCameraOutput()
        {
            if (monitorCamera)
            {
                monitorCamera.targetTexture = null;
                monitorCamera.enabled = false;
            }

            if (sourceBattleCamera && sourceBattleCamera.targetTexture == cameraRenderTexture)
                sourceBattleCamera.targetTexture = null;
        }

        private int GetNextIndex(int current, int count, bool shuffle)
        {
            if (count <= 1) return 0;

            if (!shuffle)
                return (current + 1) % count;

            int next;
            do next = Random.Range(0, count);
            while (next == current);

            return next;
        }

        // モニターの見た目を変更（MaterialPropertyBlock版）
        private void SetBaseTexture(Texture tex)
        {
            if (!screenRenderer) return;

            screenRenderer.GetPropertyBlock(propertyBlock, materialIndex);

            propertyBlock.SetTexture(texturePropertyID, tex);
            propertyBlock.SetTexture(mainTexID, tex);

            screenRenderer.SetPropertyBlock(propertyBlock, materialIndex);
        }
    }
}
