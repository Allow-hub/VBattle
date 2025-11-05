using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;

namespace TechC.VBattle.Core.Managers
{
    /// <summary>
    /// ロード画面の管理クラス
    /// </summary>
    public class LoadingManager : MonoBehaviour
    {
        [SerializeField] private Image progressImage;
        [SerializeField] private TextMeshProUGUI loadTex;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float minimumLoadingTime = 0.5f;

        private bool isInitialized = false;
        private bool isUpdatingProgress = false; // プログレス更新中フラグ

        private void Awake()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            try
            {
                if (progressImage != null)
                {
                    progressImage.fillAmount = 0f;
                }
                else
                {
                    Debug.LogWarning("ProgressImageが設定されていません");
                }

                if (loadTex != null)
                {
                    loadTex.text = "0%";
                }
                else
                {
                    Debug.LogWarning("LoadTextが設定されていません");
                }

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                }
                else
                {
                    Debug.LogWarning("CanvasGroupが設定されていません");
                }

                isInitialized = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LoadingManager初期化エラー: {e.Message}");
                isInitialized = true; // エラーが発生してもフラグを立てる
            }
        }

        private void Start()
        {
            if (canvasGroup != null)
                FadeIn().Forget();
        }

        private async UniTask FadeIn()
        {
            try
            {
                float elapsed = 0f;
                while (elapsed < fadeInDuration)
                {
                    elapsed += Time.deltaTime;
                    if (canvasGroup != null)
                    {
                        canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                    }
                    await UniTask.Yield();
                }

                if (canvasGroup != null)
                    canvasGroup.alpha = 1f;

            }
            catch (System.Exception e)
            {
                Debug.LogError($"フェードインエラー: {e.Message}");
            }
        }

        public async UniTask UpdateProgressAsync(AsyncOperation op)
        {
            // 重複実行を防止
            if (isUpdatingProgress)
            {
                Debug.LogWarning("既にプログレス更新中です");
                return;
            }

            isUpdatingProgress = true;

            try
            {

                // 初期化完了まで待機（タイムアウト付き）
                int initWaitCount = 0;
                while (!isInitialized && initWaitCount < 60) // 最大2秒待機
                {
                    await UniTask.Yield();
                    initWaitCount++;
                }

                if (!isInitialized)
                {
                    Debug.LogError("LoadingManager初期化がタイムアウトしました");
                    return;
                }

                float startTime = Time.time;
                float displayedProgress = 0f;

                // allowSceneActivationの設定を確認
                if (op.allowSceneActivation)
                {
                    op.allowSceneActivation = false;
                }

                int loopCount = 0;
                const int maxLoopCount = 3000; // 無限ループ防止（約100秒）

                // プログレス更新ループ
                while ((op.progress < 0.9f || (Time.time - startTime) < minimumLoadingTime) && loopCount < maxLoopCount)
                {
                    loopCount++;

                    // 定期的にログ出力
                    if (loopCount % 300 == 0)
                    {
                        Debug.Log($"プログレス更新中: {op.progress:F2}, 経過時間: {Time.time - startTime:F2}s");
                    }

                    // 実際のロード進捗（0〜0.9を0〜1にマッピング）
                    float actualProgress = Mathf.Clamp01(op.progress / 0.9f);

                    // 最小表示時間を考慮した進捗計算
                    float timeProgress = Mathf.Clamp01((Time.time - startTime) / minimumLoadingTime);
                    float targetProgress = Mathf.Min(actualProgress, timeProgress);

                    // スムーズな補間
                    displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, Time.deltaTime * 3f);

                    // UI更新
                    UpdateUI(displayedProgress);

                    await UniTask.Yield();
                }

                if (loopCount >= maxLoopCount)
                {
                    Debug.LogError("プログレス更新ループがタイムアウトしました");
                }

                // 最終的に100%表示
                UpdateUI(1f);

                // 少し待機してからシーン切り替えを許可
                await UniTask.Delay(200);

                op.allowSceneActivation = true;

                // シーンが実際に切り替わるまで待機
                await UniTask.WaitUntil(() => op.isDone);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"プログレス更新エラー: {e.Message}\n{e.StackTrace}");

                // エラーが発生した場合もシーン切り替えを許可
                try
                {
                    op.allowSceneActivation = true;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"allowSceneActivation設定エラー: {ex.Message}");
                }
            }
            finally
            {
                isUpdatingProgress = false;
            }
        }

        private void UpdateUI(float progress)
        {
            try
            {
                if (progressImage != null)
                    progressImage.fillAmount = progress;

                if (loadTex != null)
                    loadTex.text = $"{(progress * 100f):0}%";
            }
            catch (System.Exception e)
            {
                Debug.LogError($"UI更新エラー: {e.Message}");
            }
        }

        private void OnDestroy()
        {
            // Debug.Log("LoadingManager: OnDestroy実行");
        }
    }
}