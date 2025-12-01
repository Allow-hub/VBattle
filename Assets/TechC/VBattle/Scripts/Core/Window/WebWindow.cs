using System;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Util;
using UnityEngine;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TechC.VBattle.Core.Window
{
    /// <summary>
    /// ウェブウィンドウクラス
    /// </summary>
    public class WebWindow : NativeWindow
    {
        public string Url { get; private set; }

        private System.Diagnostics.Process _browserProcess;
        private MonoBehaviour mono;
        public HWND WebWindowHwnd => webWindow; // ウィンドウのハンドルを公開
        private HWND webWindow;// ウェブウィンドウのハンドル

        public WebWindow(IntPtr hwnd, int width, int height, MonoBehaviour mono, string url)
            : base(hwnd, width, height, WindowFactory.WindowType.Web)
        {
            Url = url;
            this.mono = mono;
            StartExe();
        }

        public override void Show()
        {
            base.Show();
            WindowUtility.SetWindowVisibility(webWindow, (int)SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE);
            // WindowUtility.SetWindowPos(
            //     webWindow,
            //     HWND.HWND_TOPMOST,
            //     0, 0, 0, 0,
            //     SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE
            // );
        }
        public override void Hide()
        {
            base.Hide();
            WindowUtility.SetWindowVisibility(webWindow, (int)SHOW_WINDOW_CMD.SW_HIDE);
        }

        public override void Destroy()
        {
            base.Destroy();

            try
            {
                if (_browserProcess != null && !_browserProcess.HasExited)
                {
                    _browserProcess.Kill();
                    _browserProcess.Dispose();
                    _browserProcess = null;
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"Failed to kill external browser: {ex}", LogTagUtil.TagWidnow);
            }
        }

        public void Move()
        {
            int currentX = 0, currentY = 0;
            var r = WindowUtility.GetWindowRect(webWindow);
            currentX = r.X;
            currentY = r.Y;

            if (Input.GetKey(KeyCode.LeftArrow)) WindowUtility.MoveWindow(webWindow, currentX - 30, currentY);
            if (Input.GetKey(KeyCode.RightArrow)) WindowUtility.MoveWindow(webWindow, currentX + 30, currentY);
            if (Input.GetKey(KeyCode.UpArrow)) WindowUtility.MoveWindow(webWindow, currentX, currentY - 30);
            if (Input.GetKey(KeyCode.DownArrow)) WindowUtility.MoveWindow(webWindow, currentX, currentY + 30);
        }

        /// <summary>
        /// 外部ブラウザを起動して指定URLを表示
        /// </summary>
        private void StartExe()
        {
            try
            {
                // Assets/WebApp/WindowsFormsApp1.exe を参照
                string exeName = "WindowsFormsApp1.exe";
                string exePath = System.IO.Path.Combine(Application.streamingAssetsPath, exeName);
                string args = $"\"{Url}\" {Hwnd} {Width} {Height}";
                _browserProcess = System.Diagnostics.Process.Start(exePath, args);
                int processId = _browserProcess.Id;
                DelayUtility.StartDelayedActionAsync(delaySeconds: 0.1f, () =>
                {
                    webWindow = WindowUtility.GetWindowByProcessId(processId);

                    // ウィンドウを最前面にする
                    WindowUtility.SetWindowPos(
                        webWindow,
                        HWND.HWND_TOPMOST,
                        0, 0, 0, 0,
                        SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
                    );
                    WindowUtility.SetWindowVisibility(webWindow, (int)SHOW_WINDOW_CMD.SW_HIDE);
                });
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"Failed to launch external browser: {ex}", LogTagUtil.TagWidnow);
            }
        }

        /// <summary>
        /// URLを設定し、WebView2に送信
        /// </summary>
        /// <param name="url"></param>
        public void SetUrl(string url = null, HtmlNames.HtmlFileName? htmlFile = null)
        {
            if (htmlFile.HasValue)
            {
                WebView2NativeMethods.SendContentToWebView2(HtmlNames.LoadHtmlFromStreamingAssets(htmlFile.Value));
                Url = url ?? htmlFile.ToString();
            }
            else if (!string.IsNullOrEmpty(url))
            {
                WebView2NativeMethods.SendContentToWebView2(url);
                Url = url;
            }
            else
                CustomLogger.Warning("SetUrl: url か htmlFile のどちらかを指定してください。", LogTagUtil.TagWidnow);
        }

        public override void SetRect()
        {
            var rect = WindowUtility.GetWindowRect(webWindow);
            SetRect(rect.Width, rect.Height);
        }
    }
}