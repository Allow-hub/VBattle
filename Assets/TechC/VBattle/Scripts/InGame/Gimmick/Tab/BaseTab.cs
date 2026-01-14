using System.Collections;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Audio;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Util;
using TechC.VBattle.Core.Window;
using UnityEngine;
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

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            rectTransform.anchoredPosition = hiddenPos;
            gameObject.SetActive(false);
            viewRect = WindowUtility.GetUnityGameViewRect();
        }

        public virtual void Show()
        {
            if(AudioManager.I != null)
                AudioManager.I.PlaySE(SEID.TabNotification);
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(SlideIn());
        }

        public virtual void Hide()
        {
            StopAllCoroutines();
            StartCoroutine(SlideOut());
        }

        public virtual void Excute() {}

        protected IEnumerator SlideIn()
        {
            SlideInWindow();
            float time = 0f;
            while (time < slideDuration)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(hiddenPos, visiblePos, time / slideDuration);
                time += Time.deltaTime;
                yield return null;
            }
            rectTransform.anchoredPosition = visiblePos;

            yield return new WaitForSeconds(visibleTime);
            Hide();
        }

        protected IEnumerator SlideOut()
        {
            SlideOutWindow();
            float time = 0f;
            while (time < slideDuration)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(visiblePos, hiddenPos, time / slideDuration);
                time += Time.deltaTime;
                yield return null;
            }
            rectTransform.anchoredPosition = hiddenPos;
            gameObject.SetActive(false);
        }

        protected virtual void SlideInWindow()
        {
            nativeWindow = WindowFactory.I.GetWindow(WindowFactory.WindowType.Image);

            // ★ hiddenPos に対応する画面座標
            int windowHiddenX = viewRect.left + (int)hiddenPos.x;
            int windowHiddenY = viewRect.top + (int)hiddenPos.y;
            // ★ visiblePos に対応する画面座標
            int windowVisibleX = viewRect.left + (int)visiblePos.x;
            int windowVisibleY = viewRect.top + (int)visiblePos.y;
            
            // ★ 最初に hiddenPos の位置に移動
            WindowUtility.MoveWindow((HWND)nativeWindow.Hwnd, windowHiddenX, windowHiddenY);
            if (!WindowUtility.ResizeWindow((HWND)nativeWindow.Hwnd, (int)windowSize.x, (int)windowSize.y))
                Debug.LogError("Windowのリサイズに失敗");
            nativeWindow.SetRect();
            
            var imageWindow = nativeWindow as ImageWindow;
            
            // ★ slideDuration に基づいてアニメーション
            float elapsedTime = 0f;
            DelayUtility.StartRepeatedActionAsync(slideDuration, repeatInterval, () =>
            {
                elapsedTime += (float)repeatInterval;
                float progress = Mathf.Clamp01(elapsedTime / slideDuration);
                
                int currentX = Mathf.RoundToInt(Mathf.Lerp(windowHiddenX, windowVisibleX, progress));
                int currentY = Mathf.RoundToInt(Mathf.Lerp(windowHiddenY, windowVisibleY, progress));
                
                imageWindow?.SetImage(windowImage.texture);
                WindowUtility.MoveWindow((HWND)imageWindow?.Hwnd, currentX, currentY);
                
                return UniTask.CompletedTask;
            }).Forget();
        }

        protected virtual void SlideOutWindow()
        {
            var imageWindow = nativeWindow as ImageWindow;
            
            // ★ visiblePos に対応する画面座標
            int windowVisibleX = viewRect.left + (int)visiblePos.x;
            int windowVisibleY = viewRect.top + (int)visiblePos.y;
            // ★ hiddenPos に対応する画面座標
            int windowHiddenX = viewRect.left + (int)hiddenPos.x;
            int windowHiddenY = viewRect.top + (int)hiddenPos.y;
            
            // ★ slideDuration に基づいてアニメーション
            float elapsedTime = 0f;
            DelayUtility.StartRepeatedActionAsync(slideDuration, repeatInterval, () =>
            {
                elapsedTime += (float)repeatInterval;
                float progress = Mathf.Clamp01(elapsedTime / slideDuration);
                
                int currentX = Mathf.RoundToInt(Mathf.Lerp(windowVisibleX, windowHiddenX, progress));
                int currentY = Mathf.RoundToInt(Mathf.Lerp(windowVisibleY, windowHiddenY, progress));
                
                imageWindow?.SetImage(windowImage.texture);
                WindowUtility.MoveWindow((HWND)imageWindow?.Hwnd, currentX, currentY);
                
                if (progress >= 1f)
                    WindowFactory.I.ReturnWindow(imageWindow);
                
                return UniTask.CompletedTask;
            }).Forget();
        }

        protected virtual void OnDestroy()
        {
            if (nativeWindow != null && WindowFactory.I != null)
                WindowFactory.I.ReturnWindow(nativeWindow);
        }
    }
}