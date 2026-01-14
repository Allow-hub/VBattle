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

        /// <summary>
        /// フォントサイズ
        /// </summary>
        public int FontSize { get; set; } = 16;

        /// <summary>
        /// テキストのX座標
        /// </summary>
        public int TextX { get; set; } = 10;

        /// <summary>
        /// テキストのY座標
        /// </summary>
        public int TextY { get; set; } = 10;

        /// <summary>
        /// フォント名
        /// </summary>
        public string FontName { get; set; } = "MS Gothic";

        /// <summary>
        /// フォントの太さ (400 = Normal, 700 = Bold)
        /// </summary>
        public int FontWeight { get; set; } = 400;

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

        /// <summary>
        /// フォント設定を変更して再描画
        /// </summary>
        /// <param name="fontSize">フォントサイズ</param>
        /// <param name="fontName">フォント名（省略可）</param>
        /// <param name="fontWeight">フォントの太さ（省略可）</param>
        public void SetFont(int fontSize, string fontName = null, int fontWeight = -1)
        {
            FontSize = fontSize;
            if (fontName != null)
                FontName = fontName;
            if (fontWeight >= 0)
                FontWeight = fontWeight;
            WindowUtility.InvalidateWindow((HWND)Hwnd);
        }

        /// <summary>
        /// テキスト描画位置を設定して再描画
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        public void SetTextPosition(int x, int y)
        {
            TextX = x;
            TextY = y;
            WindowUtility.InvalidateWindow((HWND)Hwnd);
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }
}