using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TechC.VBattle.Core.Managers
{
    /// <summary>
    /// シーン読み込みを管理するクラス
    /// Loading画面の制御、遷移時の処理、エラーハンドリングを担当
    /// </summary>
    public class SceneLoader : Singleton<SceneLoader>
    {
        #region Scene Names/Indices
        public const string LOADING_SCENE = "LoadScene";
        public const int TITLE_SCENE = 0;
        public const int SELECT_SCENE = 1;
        public const int BATTLE_SCENE = 2;
        public const int RESULT_SCENE = 3;
        #endregion

        #region Events
        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;
        public event Action<string, string> OnSceneLoadFailed;
        public event Action<float> OnLoadingProgress;
        #endregion

        #region Loading Scene Management
        [SerializeField] private bool preloadLoadingScene = true;
        
        private Scene? cachedLoadingScene = null;
        private bool isLoading = false;

        public bool IsLoading => isLoading;
        #endregion

        #region Initialization
        public override void Init()
        {
            base.Init();

            if (preloadLoadingScene)
                PreloadLoadingSceneAsync().Forget();
        }
        
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                LoadBattleSceneAsync().Forget();
            }
        }

        /// <summary>
        /// Loading画面を事前に読み込んで非アクティブ状態でキャッシュ
        /// </summary>
        private async UniTask PreloadLoadingSceneAsync()
        {
            if (SceneManager.GetSceneByName(LOADING_SCENE).isLoaded)
                return;

            try
            {
                var loadOp = SceneManager.LoadSceneAsync(LOADING_SCENE, LoadSceneMode.Additive);
                await UniTask.WaitUntil(() => loadOp.isDone);

                cachedLoadingScene = SceneManager.GetSceneByName(LOADING_SCENE);
                SetSceneActive(LOADING_SCENE, false);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SceneLoader] Failed to preload LoadingScene: {e.Message}");
            }
        }
        #endregion

        #region Scene Loading
        /// <summary>
        /// シーンをインデックスで読み込み
        /// </summary>
        public async UniTask LoadSceneAsync(int sceneIndex)
        {
            await LoadSceneInternalAsync(sceneIndex);
        }

        /// <summary>
        /// タイトルシーンへ
        /// </summary>
        public async UniTask LoadTitleSceneAsync()
        {
            await LoadSceneAsync(TITLE_SCENE);
        }

        /// <summary>
        /// セレクトシーンへ
        /// </summary>
        public async UniTask LoadSelectSceneAsync()
        {
            await LoadSceneAsync(SELECT_SCENE);
        }

        /// <summary>
        /// バトルシーンへ
        /// </summary>
        public async UniTask LoadBattleSceneAsync()
        {
            await LoadSceneAsync(BATTLE_SCENE);
        }

        /// <summary>
        /// リザルトシーンへ
        /// </summary>
        public async UniTask LoadResultSceneAsync()
        {
            await LoadSceneAsync(RESULT_SCENE);
        }
        #endregion

        #region Internal Loading Logic

        /// <summary>
        /// シーン読み込みの内部実装
        /// </summary>
        private async UniTask LoadSceneInternalAsync(int targetSceneIndex)
        {
            if (isLoading)
                return;

            isLoading = true;
            string sceneName = GetSceneNameFromIndex(targetSceneIndex);

            try
            {
                Scene previousScene = SceneManager.GetActiveScene();

                OnSceneLoadStarted?.Invoke(sceneName);

                // 遷移前処理
                await ExecuteBeforeSceneTransition(previousScene.name, sceneName);

                // === LoadingSceneの準備 ===
                Scene loadingScene;
                if (cachedLoadingScene.HasValue && cachedLoadingScene.Value.isLoaded)
                {
                    // 事前読み込み済みの場合：アクティブ化
                    loadingScene = cachedLoadingScene.Value;
                    SetSceneActive(LOADING_SCENE, true);
                    SceneManager.SetActiveScene(loadingScene);
                }
                else
                {
                    // 通常の読み込み
                    var loadLoadingSceneOp = SceneManager.LoadSceneAsync(LOADING_SCENE, LoadSceneMode.Additive);
                    await UniTask.WaitUntil(() => loadLoadingSceneOp.isDone);

                    loadingScene = SceneManager.GetSceneByName(LOADING_SCENE);
                    if (loadingScene.IsValid())
                        SceneManager.SetActiveScene(loadingScene);
                }

                // LoadingManagerの取得
                LoadingManager loadingManager = await GetLoadingManagerAsync();

                // === 前のシーンをアンロード ===
                if (previousScene.name != LOADING_SCENE)
                {
                    var unloadPrevOp = SceneManager.UnloadSceneAsync(previousScene);
                    await UniTask.WaitUntil(() => unloadPrevOp.isDone);
                }

                // === ターゲットシーン読み込み ===
                var loadTargetOp = SceneManager.LoadSceneAsync(targetSceneIndex, LoadSceneMode.Additive);

                // LoadingManagerでプログレス更新
                if (loadingManager != null)
                {
                    await loadingManager.UpdateProgressAsync(loadTargetOp);
                }
                else
                {
                    // LoadingManagerが見つからない場合は待機のみ
                    await UniTask.WaitUntil(() => loadTargetOp.isDone);
                }

                // === シーン切り替え ===
                var targetScene = SceneManager.GetSceneByBuildIndex(targetSceneIndex);
                if (targetScene.IsValid())
                {
                    SceneManager.SetActiveScene(targetScene);
                }
                else
                {
                    throw new Exception($"Loaded scene is invalid: {sceneName}");
                }

                // === LoadingSceneの処理 ===
                if (preloadLoadingScene)
                {
                    // 事前読み込みモード：非アクティブ化して保持
                    SetSceneActive(LOADING_SCENE, false);
                }
                else
                {
                    // 通常モード：アンロード
                    var unloadLoadingOp = SceneManager.UnloadSceneAsync(LOADING_SCENE);
                    await UniTask.WaitUntil(() => unloadLoadingOp.isDone);
                }

                // 遷移後処理
                await ExecuteAfterSceneTransition(previousScene.name, sceneName);

                OnSceneLoadCompleted?.Invoke(sceneName);
                Debug.Log($"[SceneLoader] Scene loaded successfully: {sceneName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SceneLoader] Failed to load scene {sceneName}: {e.Message}\n{e.StackTrace}");
                OnSceneLoadFailed?.Invoke(sceneName, e.Message);
            }
            finally
            {
                isLoading = false;
            }
        }
        #endregion

        #region Loading Manager Access
        
        /// <summary>
        /// LoadingManagerを効率的に取得
        /// </summary>
        private async UniTask<LoadingManager> GetLoadingManagerAsync()
        {
            LoadingManager loadingManager = null;
            int attempts = 0;
            const int maxAttempts = 30; // 最大1秒待機（30フレーム）

            while (loadingManager == null && attempts < maxAttempts)
            {
                loadingManager = UnityEngine.Object.FindObjectOfType<LoadingManager>();
                if (loadingManager == null)
                {
                    attempts++;
                    await UniTask.Yield();
                }
            }
            return loadingManager;
        }
        #endregion

        #region Scene Transition Hooks
        /// <summary>
        /// シーン遷移前の処理
        /// </summary>
        private async UniTask ExecuteBeforeSceneTransition(string fromScene, string toScene)
        {
            // シーン別の前処理
            switch (fromScene)
            {
                case "TitleScene":
                    // タイトル離脱時の処理
                    break;

                case "SelectScene":
                    // セレクト離脱時の処理
                    break;

                case "BattleScene":
                    // バトル離脱時の処理
                    // WindowManager.I?.ReleaseAllWindows();
                    break;

                case "ResultScene":
                    // リザルト離脱時の処理
                    break;
            }

            await UniTask.Yield();
        }

        /// <summary>
        /// シーン遷移後の処理
        /// </summary>
        private async UniTask ExecuteAfterSceneTransition(string fromScene, string toScene)
        {
            Debug.Log($"[SceneLoader] After transition: {fromScene} → {toScene}");

            // シーン別の後処理
            switch (toScene)
            {
                case "TitleScene":
                    // AudioManager.I?.PlayBGM(BGMID.Title);
                    SetCursorMode(true, CursorLockMode.None);
                    break;

                case "SelectScene":
                    SetCursorMode(true, CursorLockMode.None);
                    break;

                case "BattleScene":
                    // AudioManager.I?.PlayBGM(BGMID.Battle);
                    SetCursorMode(false, CursorLockMode.Locked);
                    break;

                case "ResultScene":
                    SetCursorMode(true, CursorLockMode.None);
                    break;
            }

            await UniTask.Yield();
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// シーンのルートオブジェクトをアクティブ/非アクティブ化
        /// </summary>
        private void SetSceneActive(string sceneName, bool active)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
            {
                foreach (GameObject obj in scene.GetRootGameObjects())
                {
                    obj.SetActive(active);
                }
            }
        }

        /// <summary>
        /// カーソル表示設定
        /// </summary>
        private void SetCursorMode(bool visible, CursorLockMode lockMode)
        {
            Cursor.visible = visible;
            Cursor.lockState = lockMode;
        }

        /// <summary>
        /// シーンインデックスから名前を取得
        /// </summary>
        private string GetSceneNameFromIndex(int index)
        {
            return index switch
            {
                TITLE_SCENE => "TitleScene",
                SELECT_SCENE => "SelectScene",
                BATTLE_SCENE => "BattleScene",
                RESULT_SCENE => "ResultScene",
                _ => $"Scene{index}"
            };
        }
        #endregion
    }
}