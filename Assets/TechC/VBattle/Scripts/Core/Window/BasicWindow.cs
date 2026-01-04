using System;
using TechC.VBattle.Core.Extensions;
using Windows.Win32.Foundation;

namespace TechC.VBattle.Core.Window
{
    /// <summary>
    /// 通常のウィンドウ
    /// </summary>
    public class BasicWindow : NativeWindow
    {
        /// <summary>
        /// ウィンドウに表示するテキスト
        /// </summary>
        public string DisplayText { get; set; } = "クラシック風ウィンドウ";

        public BasicWindow(IntPtr hwnd, int width, int height) : base(hwnd, width, height, WindowFactory.WindowType.Basic)
        {
        }

        public override void Show()
        {
            base.Show();
            CustomLogger.Info($"[Show] hwnd: {Hwnd}", LOGTAG);
            // 表示時に再描画してテキストを反映
            WindowUtility.InvalidateWindow((HWND)Hwnd);
        }

        public override void Hide()
        {
            base.Hide();
            CustomLogger.Info($"[Hide] hwnd: {Hwnd}", LOGTAG);
        }

        public override void SetRect()
        {
            base.SetRect();
            CustomLogger.Info($"[SetRect] hwnd: {Hwnd}, Width: {Width}, Height: {Height}", LOGTAG);
        }
        
        public void ResizeWindow(int width, int height, int delay = 0)
        {
            base.ResizeWindow(width, height, delay);
            CustomLogger.Info($"[ResizeWindow] hwnd: {Hwnd}, Width: {Width}, Height: {Height}", LOGTAG);
        }

        /// <summary>
        /// 表示テキストを設定して再描画
        /// </summary>
        /// <param name="text">表示するテキスト</param>
        public void SetText(string text)
        {
            DisplayText = text;
            WindowUtility.InvalidateWindow((HWND)Hwnd);
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }
}