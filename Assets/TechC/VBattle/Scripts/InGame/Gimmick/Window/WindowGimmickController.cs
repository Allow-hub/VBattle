using Cysharp.Threading.Tasks;
using System;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Util;
using TechC.VBattle.Core.Window;
using UnityEngine;
using Windows.Win32.Foundation;

namespace TechC.VBattle.InGame.Gimmick
{
    /// <summary>
    /// ウィンドウのギミック管理
    /// </summary>
    [Serializable]
    public class WindowGimmickController : IGimmick
    {
        [SerializeField] private LayerMask groundLayer;

        [SerializeField] private Vector2 intervalRange;
        [SerializeField] private float appearTime = 5f;
        [SerializeField] private Sprite wallSprite;
        private int initWindowPosX = -50;
        private int initWindowPosY = -50;
        private float timer;
        private float currentInterval;
        private bool isEventRunning = false;
        private NativeWindow nativeWindow;
        private int height = 100;
        private const float lowerTargetRate = 0.5f;


        /// <summary>
        /// ウィンドウの出現方向
        /// </summary>
        public enum WindowDirection
        {
            LeftToRight,  // 左から右
            RightToLeft,  // 右から左
            TopToBottom,  // 上から下
            BottomToTop   // 下から上
        }

        public void OnEnter()
        {
            timer = 0f;
            currentInterval = UnityEngine.Random.Range(intervalRange.x, intervalRange.y);
        }

        public void OnUpdate(float deltaTime)
        {
            if (isEventRunning) return; // イベント中はタイマー止める

            timer += deltaTime;
            if (timer >= currentInterval)
            {
                isEventRunning = true;
                ExcuteEvent();

                currentInterval = UnityEngine.Random.Range(intervalRange.x, intervalRange.y);
                timer = 0f;
            }
        }

        public void OnExit()
        {
            if (nativeWindow != null && WindowFactory.I != null)
            {
                WindowFactory.I.ReturnWindow(nativeWindow);
            }
        }

        private void ExcuteEvent()
        {
            // ランダムに方向を選択（4方向）
            WindowDirection direction = (WindowDirection)UnityEngine.Random.Range(0, 4);

            // if (GameManager.I.CanConectWifi)
            // {
            //     nativeWindow = WindowFactory.I.GetWindow(WindowFactory.WindowType.Web);
            //     var web = nativeWindow as WebWindow;
            //     if (direction == WindowDirection.BottomToTop)
            //         WindowManager.I.AddColliderWindow(nativeWindow, groundLayer);
            //     else
            //         WindowManager.I.AddColliderWindow(nativeWindow);

            //     SetupWindowByDirection(web.WebWindowHwnd, direction);
            //     web.SetUrl(null, HtmlNames.HtmlFileName.Wall);
            //     MoveWindowByDirection(web, direction).Forget();
            // }
            // else
            // {
            nativeWindow = WindowFactory.I.GetWindow(WindowFactory.WindowType.Image);
            if (direction == WindowDirection.BottomToTop)
                WindowManager.I.AddColliderWindow(nativeWindow, groundLayer);
            else
                WindowManager.I.AddColliderWindow(nativeWindow);
            SetupWindowByDirection((HWND)nativeWindow.Hwnd, direction);
            var image = nativeWindow as ImageWindow;
            nativeWindow.SetRect();
            image.SetImage(wallSprite.texture);
            MoveWindowByDirectionWithTexture(nativeWindow, direction).Forget();
            // }

            DelayUtility.StartDelayedActionWithPauseAsync(appearTime, () =>
            {
                WindowFactory.I.ReturnWindow(nativeWindow);
                WindowManager.I.RemoveColliderWindow(nativeWindow);
                isEventRunning = false;
            }, InGameManager.I.GetPauseStateFunc);
        }

        /// <summary>
        /// 方向に応じてウィンドウの初期サイズと位置を設定
        /// </summary>
        private void SetupWindowByDirection(HWND hwnd, WindowDirection direction)
        {
            switch (direction)
            {
                case WindowDirection.LeftToRight:
                    WindowUtility.ResizeWindow(hwnd, 10, Screen.height);
                    WindowUtility.MoveWindow(hwnd, initWindowPosX, 0);
                    break;

                case WindowDirection.RightToLeft:
                    WindowUtility.ResizeWindow(hwnd, 10, Screen.height);
                    WindowUtility.MoveWindow(hwnd, Screen.width + Math.Abs(initWindowPosX), 0);
                    break;

                case WindowDirection.TopToBottom:
                    WindowUtility.ResizeWindow(hwnd, Screen.width, height);
                    WindowUtility.MoveWindow(hwnd, 0, initWindowPosY);
                    break;

                case WindowDirection.BottomToTop:
                    WindowUtility.ResizeWindow(hwnd, Screen.width, height);
                    WindowUtility.MoveWindow(hwnd, 0, Screen.height + Math.Abs(initWindowPosY));
                    break;
            }
        }

        /// <summary>
        /// 方向に応じてWebウィンドウを移動
        /// </summary>
        private async UniTask MoveWindowByDirection(WebWindow web, WindowDirection direction)
        {
            web.SetRect();
            switch (direction)
            {
                case WindowDirection.LeftToRight:
                    await WindowUtility.MoveWindowToTargetAsync(web, Screen.width / 3, 0);
                    break;

                case WindowDirection.RightToLeft:
                    await WindowUtility.MoveWindowToTargetAsync(web, Screen.width * 2 / 3, 0);
                    break;

                case WindowDirection.TopToBottom:
                    await WindowUtility.MoveWindowToTargetAsync(web, 0, Screen.height / 3);
                    break;

                case WindowDirection.BottomToTop:
                    await WindowUtility.MoveWindowToTargetAsync(web, 0, Screen.height * 2 / 3);
                    break;
            }
        }

        private Vector2Int GetTargetPosition(WindowDirection direction)
        {
            return direction switch
            {
                WindowDirection.LeftToRight => new Vector2Int(Screen.width / 3, 0),
                WindowDirection.RightToLeft => new Vector2Int(Screen.width * 2 / 3, 0),
                WindowDirection.TopToBottom => new Vector2Int(0, Screen.height / 6),
                WindowDirection.BottomToTop => new Vector2Int(0, (int)(Screen.height * lowerTargetRate)),//1080 × 0.57 = 615
                _ => Vector2Int.zero
            };
        }

        /// <summary>
        /// 方向に応じてImageウィンドウを移動（テクスチャ付き）
        /// </summary>
        private async UniTask MoveWindowByDirectionWithTexture(NativeWindow window, WindowDirection direction)
        {
            var targetPos = GetTargetPosition(direction);

            int moveSpeed = (direction == WindowDirection.LeftToRight || direction == WindowDirection.RightToLeft) ? 10 : 16;
            int interval = (direction == WindowDirection.LeftToRight || direction == WindowDirection.RightToLeft) ? 16 : 10;

            await WindowUtility.MoveWindowToTargetAsync(
                window,
                targetPos.x,
                targetPos.y,
                moveSpeedPerFrame: moveSpeed,
                intervalMs: interval,
                wallSprite.texture
            );
        }
    }
}