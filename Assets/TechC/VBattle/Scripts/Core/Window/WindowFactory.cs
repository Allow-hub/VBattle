using System;
using System.Collections.Generic;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Managers;
using UnityEngine;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TechC.VBattle.Core.Window
{
    /// <summary>
    /// ウィンドウを生成するFactoryクラス
    /// </summary>
    public class WindowFactory : Singleton<WindowFactory>
    {
        public enum WindowType { Basic, Image, Web }

        private Dictionary<WindowType, Queue<NativeWindow>> poolByType = new();
        private int InitialPoolSize = 1;
        private Vector2 defaultWindowSize = new Vector2(300, 300); 
        Dictionary<WindowType, int> initialPoolSizes = new Dictionary<WindowType, int>
        {
            { WindowType.Basic, 2 },
            { WindowType.Image, 81 },
            { WindowType.Web,   0 }
        };
        private List<NativeWindow> activeWindows = new();

        public override void Init()
        {
            base.Init();
            CustomWindowUtility.RegisterWindowClasses();

            // ウィンドウタイプごとの初期プールサイズを定義
            foreach (WindowType type in Enum.GetValues(typeof(WindowType)))
            {
                poolByType[type] = new Queue<NativeWindow>();
                int poolSize = initialPoolSizes.TryGetValue(type, out var size) ? size : InitialPoolSize;
                for (int i = 0; i < poolSize; i++)
                {
                    var window = CreateNewWindow(type, $"{type} Window {i}", (int)defaultWindowSize.x, (int)defaultWindowSize.y);
                    if (window != null)
                        poolByType[type].Enqueue(window);
                }
            }
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            DisposeAll();
            CustomWindowUtility.UnregisterWindowClasses();
        }
        
        public NativeWindow GetWindow(WindowType type)
        {
            NativeWindow window = null;

            if (poolByType.TryGetValue(type, out var queue) && queue.Count > 0)
            {
                window = queue.Dequeue(); // 再利用
            }
            else
            {
                window = CreateNewWindow(type, $"{type} Window", 100, 100); // 必要なら新規作成
            }

            // ウィンドウを表示
            if (window != null)
            {
                window.Show();
                if (!activeWindows.Contains(window))
                    activeWindows.Add(window);
            }

            return window;
        }
        public void ReturnWindow(NativeWindow window)
        {
            window.Hide();
            if (!poolByType.ContainsKey(window.Type))
            {
                poolByType[window.Type] = new Queue<NativeWindow>();
            }
            poolByType[window.Type].Enqueue(window);
            activeWindows.Remove(window);
        }

        public NativeWindow CreateNewWindow() =>
            CreateNewWindow(WindowType.Basic, "Default", 100, 100);


        /// <summary>
        /// 新しいウィンドウの作成
        /// </summary>
        /// <param name="type">ウィンドウタイプ</param>
        /// <param name="title">ウィンドウ名</param>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        /// <param name="tex">ImageTypeの場合必要</param>
        /// <returns></returns>
        public NativeWindow CreateNewWindow(WindowType type, string title, int width, int height, Texture2D tex = null)
        {
            // ウィンドウクラス名を決定
            string className = type switch
            {
                WindowType.Image => "WindowClass_Image",
                WindowType.Web => "WindowClass_Web",
                _ => "WindowClass_Basic",
            };

            uint style, exStyle;
            if (type == WindowType.Web)
            {
                // WebWindowのみ: 枠なし・非アクティブ・最前面・ツールウィンドウ・入力透過
                style = (uint)WINDOW_STYLE.WS_POPUP;
                exStyle = (uint)(
                    WINDOW_EX_STYLE.WS_EX_NOACTIVATE |
                    WINDOW_EX_STYLE.WS_EX_TOPMOST |
                    WINDOW_EX_STYLE.WS_EX_TOOLWINDOW |
                    WINDOW_EX_STYLE.WS_EX_LAYERED |
                    WINDOW_EX_STYLE.WS_EX_TRANSPARENT
                );
            }
            else
            {
                // 通常ウィンドウ
                style = (uint)WINDOW_STYLE.WS_OVERLAPPEDWINDOW;
                // exStyle = (uint)WINDOW_EX_STYLE.WS_EX_NOACTIVATE | (uint)WINDOW_EX_STYLE.WS_EX_TOPMOST | (uint)WINDOW_EX_STYLE.WS_EX_LAYERED;
                exStyle = (uint)WINDOW_EX_STYLE.WS_EX_NOACTIVATE | (uint)WINDOW_EX_STYLE.WS_EX_TOPMOST;
            }

            IntPtr hwnd = CustomWindowUtility.CreateWindow(
                className,
                title,
                style,
                exStyle,
                100, -500, width, height,//作成時は画面外に
                IntPtr.Zero
            );

            if (hwnd == IntPtr.Zero)
                return null;

            // switch (type)
            // {
            //     case WindowType.Basic:
            //         return new BasicWindow(hwnd, width, height);
            //     case WindowType.Image:
            //         return new ImageWindow(hwnd, width, height, tex);
            //     case WindowType.Web:
            //         // WebWindowはURLを設定する必要があるため、初期URLを指定
            //         return new WebWindow(hwnd, width, height, this, "https://www.google.com");
            //     default:
            //         return new NativeWindow(hwnd, width, height, type);
            // }
        }

        /// <summary>
        /// 全削除
        /// </summary>
        public void DisposeAll()
        {
            CustomLogger.Info($"DisposeAll called. Pool count: {poolByType.Count}", WindowUtility.WINDOWLOGTAG);

            // プール内
            foreach (var kvp in poolByType)
            {
                var type = kvp.Key;
                var queue = kvp.Value;
                CustomLogger.Info($"Disposing windows of type: {type}, count: {queue.Count}", WindowUtility.WINDOWLOGTAG);

                foreach (var window in queue)
                {
                    CustomLogger.Info($"Destroying window: HWND={window.Hwnd}, Type={window.Type}", WindowUtility.WINDOWLOGTAG);
                    if (window.Hwnd != IntPtr.Zero)
                    {
                        window.Destroy();
                    }
                }
                queue.Clear();
            }

            // アクティブウィンドウ
            foreach (var window in activeWindows)
            {
                CustomLogger.Info($"Destroying active window: HWND={window.Hwnd}, Type={window.Type}", WindowUtility.WINDOWLOGTAG);
                if (window.Hwnd != IntPtr.Zero)
                {
                    window.Destroy();
                }
            }
            activeWindows.Clear();
        }

    }
}