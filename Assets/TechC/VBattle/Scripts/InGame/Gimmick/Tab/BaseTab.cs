using System.Collections;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Audio;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Util;
using TechC.VBattle.Core.Window;
using UnityEngine;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace TechC.VBattle.InGame.Gimmick.Tab
{
    public enum TabType
    {
        Normal,//コメント早くなるやつ
    }

    /// <summary>
    /// タブの基底クラス
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BaseTab : MonoBehaviour, ITab
    {
        [SerializeField] private Sprite windowImage;
        [SerializeField] protected float slideDuration = 0.5f;    // スライドアニメ時間
        [SerializeField] protected float visibleTime = 3f;        // 表示持続時間
        public float VisibleTime => visibleTime;

        protected RectTransform rectTransform;
        [SerializeField] protected Vector2 hiddenPos = new Vector2(0, 100);   // 画面外（上）
        [SerializeField] protected Vector2 visiblePos = new Vector2(0, -50);  // 表示位置
        [SerializeField] protected Vector2 windowSize = new Vector2(400, 200);
        public TabType TabType => tabType;
        protected TabType tabType;

        private float repeatInterval = 0.01f;
        private NativeWindow nativeWindow;
        private RECT viewRect;
        
        // ウィンドウの現在位置を保持
        private int currentWindowX;
        private int currentWindowY;
        
        // アニメーション実行中かどうかのフラグ
        private bool isAnimating = false;
        
        // ★ アニメーションのキャンセル用
        private bool cancelAnimation = false;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            rectTransform.anchoredPosition = hiddenPos;
            gameObject.SetActive(false);
            viewRect = WindowUtility.GetUnityGameViewRect();
        }

        public virtual void Show()
        {
            // 既にアニメーション中または表示中の場合は何もしない
            if (isAnimating || gameObject.activeSelf)
            {
                Debug.LogWarning("Tab is already showing or animating");
                return;
            }
            
            if(AudioManager.I != null)
                AudioManager.I.PlaySE(SEID.TabNotification);
            
            gameObject.SetActive(true);
            StopAllCoroutines();
            cancelAnimation = false; // ★ キャンセルフラグをリセット
            StartCoroutine(SlideIn());
        }

        public virtual void Hide()
        {
            // アニメーション中でない、または既に非表示の場合は何もしない
            if (!gameObject.activeSelf)
                return;
            
            StopAllCoroutines();
            cancelAnimation = false; // ★ キャンセルフラグをリセット
            StartCoroutine(SlideOut());
        }

        public virtual void Excute() {}

        protected IEnumerator SlideIn()
        {
            isAnimating = true;
            
            // ★ Windowアニメーションを開始（awaitで完了を待つ）
            yield return SlideInWindowCoroutine();
            
            isAnimating = false;

            yield return new WaitForSeconds(visibleTime);
            Hide();
        }

        protected IEnumerator SlideOut()
        {
            isAnimating = true;
            
            // ★ Windowアニメーションを開始（awaitで完了を待つ）
            yield return SlideOutWindowCoroutine();
            
            isAnimating = false;
            gameObject.SetActive(false);
        }

        // ★ SlideInをコルーチンに変更
        protected IEnumerator SlideInWindowCoroutine()
        {
            // 既存のウィンドウがあれば先に破棄
            if (nativeWindow != null)
            {
                Debug.LogWarning("Previous window still exists, returning it first");
                WindowFactory.I.ReturnWindow(nativeWindow);
                nativeWindow = null;
            }
            
            nativeWindow = WindowFactory.I.GetWindow(WindowFactory.WindowType.Image);

            // hiddenPos に対応する画面座標
            int windowHiddenX = viewRect.left + (int)hiddenPos.x;
            int windowHiddenY = viewRect.top + (int)hiddenPos.y;
            // visiblePos に対応する画面座標
            int windowVisibleX = viewRect.left + (int)visiblePos.x;
            int windowVisibleY = viewRect.top + (int)visiblePos.y;
            
            // 最初に hiddenPos の位置に移動
            WindowUtility.MoveWindow((HWND)nativeWindow.Hwnd, windowHiddenX, windowHiddenY);
            if (!WindowUtility.ResizeWindow((HWND)nativeWindow.Hwnd, (int)windowSize.x, (int)windowSize.y))
                Debug.LogError("Windowのリサイズに失敗");
            nativeWindow.SetRect();
            
            var imageWindow = nativeWindow as ImageWindow;
            
            // ★ UIとWindowを同時にアニメーション
            float elapsedTime = 0f;
            while (elapsedTime < slideDuration)
            {
                if (cancelAnimation || imageWindow == null || nativeWindow == null)
                    yield break;
                
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / slideDuration);
                
                // UI のアニメーション
                rectTransform.anchoredPosition = Vector2.Lerp(hiddenPos, visiblePos, progress);
                
                // Window のアニメーション
                currentWindowX = Mathf.RoundToInt(Mathf.Lerp(windowHiddenX, windowVisibleX, progress));
                currentWindowY = Mathf.RoundToInt(Mathf.Lerp(windowHiddenY, windowVisibleY, progress));
                
                imageWindow?.SetImage(windowImage.texture);
                WindowUtility.MoveWindow((HWND)imageWindow?.Hwnd, currentWindowX, currentWindowY);
                
                yield return null;
            }
            
            // ★ 最終位置を確実に設定
            rectTransform.anchoredPosition = visiblePos;
            currentWindowX = windowVisibleX;
            currentWindowY = windowVisibleY;
            if (imageWindow != null)
            {
                WindowUtility.MoveWindow((HWND)imageWindow.Hwnd, currentWindowX, currentWindowY);
                imageWindow.SetImage(windowImage.texture);
            }
        }

        // ★ SlideOutをコルーチンに変更
        protected IEnumerator SlideOutWindowCoroutine()
        {
            // ウィンドウが存在しない場合は何もしない
            if (nativeWindow == null)
            {
                Debug.LogWarning("No window to slide out");
                yield break;
            }
            
            var imageWindow = nativeWindow as ImageWindow;
            
            // 現在の位置を取得
            RECT windowRect;
            if (PInvoke.GetWindowRect((HWND)nativeWindow.Hwnd, out windowRect))
            {
                currentWindowX = windowRect.left;
                currentWindowY = windowRect.top;
            }
            else
            {
                currentWindowX = viewRect.left + (int)visiblePos.x;
                currentWindowY = viewRect.top + (int)visiblePos.y;
            }
            
            // hiddenPos に対応する画面座標
            int windowHiddenX = viewRect.left + (int)hiddenPos.x;
            int windowHiddenY = viewRect.top + (int)hiddenPos.y;
            
            // ★ UIとWindowを同時にアニメーション
            float elapsedTime = 0f;
            while (elapsedTime < slideDuration)
            {
                if (cancelAnimation || imageWindow == null || nativeWindow == null)
                    yield break;
                
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / slideDuration);
                
                // UI のアニメーション
                rectTransform.anchoredPosition = Vector2.Lerp(visiblePos, hiddenPos, progress);
                
                // Window のアニメーション
                int targetX = Mathf.RoundToInt(Mathf.Lerp(currentWindowX, windowHiddenX, progress));
                int targetY = Mathf.RoundToInt(Mathf.Lerp(currentWindowY, windowHiddenY, progress));
                
                imageWindow?.SetImage(windowImage.texture);
                WindowUtility.MoveWindow((HWND)imageWindow?.Hwnd, targetX, targetY);
                
                yield return null;
            }
            
            // ★ 最終位置を確実に設定
            rectTransform.anchoredPosition = hiddenPos;
            
            // ★ アニメーション完了後にウィンドウを破棄
            if (nativeWindow != null && WindowFactory.I != null)
            {
                WindowFactory.I.ReturnWindow(nativeWindow);
                nativeWindow = null;
            }
        }

        protected virtual void OnDestroy()
        {
            // 破棄時に確実にウィンドウをクリーンアップ
            if (nativeWindow != null && WindowFactory.I != null)
            {
                WindowFactory.I.ReturnWindow(nativeWindow);
                nativeWindow = null;
            }
        }
        
        // OnDisable時もクリーンアップ
        protected virtual void OnDisable()
        {
            StopAllCoroutines();
            cancelAnimation = true; // ★ アニメーションをキャンセル
            
            // アニメーション中のウィンドウを即座に破棄
            if (nativeWindow != null && WindowFactory.I != null)
            {
                WindowFactory.I.ReturnWindow(nativeWindow);
                nativeWindow = null;
            }
            
            isAnimating = false;
        }
    }
}