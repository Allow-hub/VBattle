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
        public TabType TabType => tabType;
        protected TabType tabType;

        private float lerpSpeed = 0.3f;
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

        public virtual void Excute()
        {

        }

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
            WindowUtility.MoveWindow((HWND)nativeWindow.Hwnd, viewRect.top, -viewRect.Height / 3);
            if (!WindowUtility.ResizeWindow((HWND)nativeWindow.Hwnd, viewRect.Width, (int)(viewRect.Height / 3.5f)))
            {
                Debug.LogError("Windowのリサイズに失敗");
            }
            nativeWindow.SetRect();
            var imageWindow = nativeWindow as ImageWindow;
            DelayUtility.StartRepeatedActionAsync(slideDuration, repeatInterval, () =>
            {
                imageWindow.SetImage(windowImage.texture);
                WindowUtility.MoveWindowToTargetPosition((HWND)imageWindow.Hwnd, viewRect.top, viewRect.top, lerpSpeed);
                return UniTask.CompletedTask;
            });
        }
        protected virtual void SlideOutWindow()
        {
            var imageWindow = nativeWindow as ImageWindow;
            DelayUtility.StartRepeatedActionAsync(slideDuration, repeatInterval, () =>
            {
                imageWindow.SetImage(windowImage.texture);
                WindowUtility.MoveWindowToTargetPosition((HWND)imageWindow.Hwnd, viewRect.top, -viewRect.Height / 4, lerpSpeed);
                if (WindowUtility.GetWindowRect((HWND)imageWindow.Hwnd).Y == -viewRect.Height / 4)
                    WindowFactory.I.ReturnWindow(imageWindow);
                return UniTask.CompletedTask;
            });
        }

        protected virtual void OnDestroy()
        {
            if (nativeWindow != null && WindowFactory.I != null)
                WindowFactory.I.ReturnWindow(nativeWindow);
        }
    }
}