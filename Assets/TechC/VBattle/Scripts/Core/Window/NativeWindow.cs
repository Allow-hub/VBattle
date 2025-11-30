using System;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Util;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TechC.VBattle.Core.Window
{
    /// <summary>
    /// 各ウィンドウの基底クラス
    /// </summary>
    public class NativeWindow
    {
        public IntPtr Hwnd { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public WindowFactory.WindowType Type { get; }

        protected static readonly string LOGTAG = LogTagUtil.TagWidnow;

        public NativeWindow(IntPtr hwnd, int width, int height, WindowFactory.WindowType type)
        {
            Hwnd = hwnd;
            Width = width;
            Height = height;
            Type = type;
        }

        /// <summary>
        /// ウィンドウの表示・非表示・破棄を行う。
        /// Show()で表示、Hide()で非表示、Destroy()で破棄される。
        /// </summary>
        public virtual void Show() => WindowUtility.SetWindowVisibility(Hwnd, (int)SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE);
        public virtual void Hide() => WindowUtility.SetWindowVisibility(Hwnd, (int)SHOW_WINDOW_CMD.SW_HIDE);
        public virtual void Destroy()
        {
            Hide();
            CustomLogger.Info($"[Destroy] hwnd: {Hwnd}", LOGTAG);

            if (Hwnd == IntPtr.Zero)
            {
                CustomLogger.Warning("Hwnd is zero before destroy", LOGTAG);
                return;
            }
            bool isWindow = WindowUtility.IsValidWindow((HWND)Hwnd);
            CustomLogger.Info($"IsWindow before destroy: {isWindow}", LOGTAG);

            bool success = WindowUtility.DestroyWindowHandle(Hwnd);
            CustomLogger.Info($"DestroyWindowHandle success: {success}", LOGTAG);
            Hwnd = IntPtr.Zero;
        }

        /// <summary>
        /// WidthとHeightを設定する。
        /// </summary>
        public virtual void SetRect()
        {
            var rect = WindowUtility.GetWindowRect((HWND)Hwnd);
            Width = rect.Width;
            Height = rect.Height;
        }

        public virtual void MoveWindowToTargetPosition(IntPtr hWnd, int targetX, int targetY, float speed) => WindowUtility.MoveWindowToTargetPosition(hWnd, targetX, targetY, speed);
        public virtual void ResizeWindow(int targetWidth, int targetHeight, float speed) => WindowUtility.AnimateResizeWindow(Hwnd, targetWidth, targetHeight, speed);

        /// <summary>
        /// ウィンドウの左上座標（スクリーン座標）を取得する
        /// </summary>
        public virtual (int x, int y) GetScreenPosition()
        {
            var rect = WindowUtility.GetWindowRect((HWND)Hwnd);
            return (rect.X, rect.Y);
        }

        public void SetRect(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}