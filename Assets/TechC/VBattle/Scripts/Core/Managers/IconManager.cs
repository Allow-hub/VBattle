using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Windows.Win32.Foundation;
using Windows.Win32;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using TechC.VBattle.Core.Extensions;

namespace TechC.VBattle.Core.Managers
{
    /// <summary>
    /// 通知領域アイコンを表示
    /// </summary>
    public class IconManager : Singleton<IconManager>
    {
        [SerializeField, ReadOnly] private const string LOGTAG = "icon";

        // メニューアイテムID
        private const uint IDM_EXIT = 0;
        private const uint IDM_HELLO = 1;
        private const uint WM_TRAYICON = PInvoke.WM_APP + 1;

        // インスタンス変数
        private HWND hWnd;
        private HICON hIcon;
        private NOTIFYICONDATAW nid;
        private bool isInitializedIcon = false;

        private WNDPROC wndProcDelegate;
        private string windowClassName = "TechCHelperWindow";

        /// <summary>
        /// シングルトンの初期化
        /// </summary>
        public override void Init()
        {
            base.Init();
            string tooltipText = "V-LinkBattle";
            CreateNotificationIcon(tooltipText);
        }

        /// <summary>
        /// アプリケーション終了時に呼ばれるUnityイベント
        /// </summary>
        private void OnApplicationQuit()
        {
            RemoveNotificationIcon();
        }

        /// <summary>
        /// 通知アイコンを作成します
        /// </summary>
        /// <param name="tooltipText">ツールチップテキスト（省略可）</param>
        public void CreateNotificationIcon(string tooltipText = "Unity アプリケーション")
        {
            try
            {
                if (isInitializedIcon)
                {
                    CustomLogger.Info("通知アイコンは既に初期化済みです", LOGTAG);
                    return;
                }

                CustomLogger.Info("通知アイコンの作成を開始します", LOGTAG);

                // ウィンドウプロシージャデリゲートを最初に作成（GC対策）
                wndProcDelegate = WindowProc;

                // 独自の非表示ウィンドウを作成
                CreateHelperWindow();

                if (hWnd == IntPtr.Zero)
                {
                    CustomLogger.Error("ヘルパーウィンドウの作成に失敗しました", LOGTAG);
                    return;
                }

                CustomLogger.Info("ヘルパーウィンドウを作成しました: " + hWnd, LOGTAG);
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

                unsafe
                {
                    fixed (char* exePathPtr = exePath)
                    {
                        PCWSTR path = new PCWSTR(exePathPtr);
                        hIcon = PInvoke.ExtractIcon(HINSTANCE.Null, path, 0);
                    }
                }

                if (hIcon == IntPtr.Zero)
                {
                    CustomLogger.Error("アイコンの読み込みに失敗しました", LOGTAG);
                    return;
                }

                CustomLogger.Info("アイコンを読み込みました: " + hIcon, LOGTAG);

                // 通知アイコンデータの構造体を初期化
                nid = new NOTIFYICONDATAW
                {
                    cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>(),
                    hWnd = hWnd,
                    uID = 1,
                    uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP,
                    uCallbackMessage = WM_TRAYICON,
                    hIcon = hIcon,
                    szTip = tooltipText
                };

                // 通知アイコンの追加
                bool result = PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_ADD, in nid);
                if (result)
                {
                    isInitializedIcon = true;
                    CustomLogger.Info("通知アイコンが正常に追加されました", LOGTAG);
                }
                else
                {
                    CustomLogger.Error("通知アイコンの追加に失敗しました", LOGTAG);
                    CleanupResources();
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error("通知アイコン作成中に例外が発生しました: " + ex.Message + "\n" + ex.StackTrace, LOGTAG);
                CleanupResources();
            }
        }

        /// <summary>
        /// 独自ウィンドウクラスを登録します
        /// </summary>
        private bool RegisterWindowClass()
        {
            unsafe
            {
                fixed (char* classNamePtr = windowClassName)
                {
                    var wc = new WNDCLASSEXW
                    {
                        cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
                        style = 0,
                        lpfnWndProc = wndProcDelegate, // インスタンス変数を使用
                        cbClsExtra = 0,
                        cbWndExtra = 0,
                        hInstance = PInvoke.GetModuleHandle(new PCWSTR(null)),
                        hIcon = HICON.Null,
                        hCursor = PInvoke.LoadCursor(HINSTANCE.Null, PInvoke.IDC_ARROW),
                        hbrBackground = new Windows.Win32.Graphics.Gdi.HBRUSH(IntPtr.Zero),
                        lpszMenuName = new PCWSTR(null),
                        lpszClassName = new PCWSTR(classNamePtr),
                        hIconSm = HICON.Null
                    };

                    ushort atom = PInvoke.RegisterClassEx(in wc);
                    if (atom == 0)
                    {
                        int error = Marshal.GetLastWin32Error();
                        CustomLogger.Error($"RegisterClassEx failed with error: {error}", LOGTAG);
                        return false;
                    }
                    CustomLogger.Info("ウィンドウクラスを正常に登録しました", LOGTAG);
                    return true;
                }
            }
        }

        /// <summary>
        /// ウィンドウクラスの登録を解除します
        /// </summary>
        private void UnregisterWindowClass()
        {
            unsafe
            {
                fixed (char* classNamePtr = windowClassName)
                {
                    bool result = PInvoke.UnregisterClass(
                        new PCWSTR(classNamePtr),
                        PInvoke.GetModuleHandle(new PCWSTR(null))
                    );
                    if (result)
                    {
                        CustomLogger.Info("ウィンドウクラスの登録を解除しました", LOGTAG);
                    }
                }
            }
        }

        /// <summary>
        /// 通知アイコンを削除します
        /// </summary>
        public void RemoveNotificationIcon()
        {
            if (isInitializedIcon)
            {
                CustomLogger.Info("通知アイコンを削除します", LOGTAG);
                if (!PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, in nid))
                    CustomLogger.Info("通知アイコンの削除に失敗しました", LOGTAG);
                isInitializedIcon = false;
            }
            CleanupResources();
        }

        /// <summary>
        /// リソースのクリーンアップを行います
        /// </summary>
        private void CleanupResources()
        {
            // アイコンハンドルを解放
            if (hIcon != IntPtr.Zero)
            {
                PInvoke.DestroyIcon((HICON)hIcon);
                hIcon = HICON.Null;
            }

            // ヘルパーウィンドウを破棄
            if (hWnd != IntPtr.Zero)
            {
                CustomLogger.Info("ヘルパーウィンドウを削除します", LOGTAG);
                PInvoke.DestroyWindow(hWnd);
                hWnd = HWND.Null;
            }

            // ウィンドウクラスの登録解除
            UnregisterWindowClass();

            // デリゲート参照をクリア
            wndProcDelegate = null;
        }

        /// <summary>
        /// 通知アイコンのメッセージ処理用ヘルパーウィンドウを作成します
        /// </summary>
        private void CreateHelperWindow()
        {
            // ウィンドウクラスを登録
            if (!RegisterWindowClass())
            {
                CustomLogger.Error("ウィンドウクラスの登録に失敗しました", LOGTAG);
                return;
            }

            unsafe
            {
                fixed (char* classNamePtr = windowClassName)
                fixed (char* windowNamePtr = "TechCHelperWindow")
                {
                    hWnd = PInvoke.CreateWindowEx(
                        WINDOW_EX_STYLE.WS_EX_TOOLWINDOW,
                        (PCWSTR)classNamePtr,
                        (PCWSTR)windowNamePtr,
                        WINDOW_STYLE.WS_OVERLAPPED,
                        0, 0, 1, 1,
                        default,
                        default,
                        PInvoke.GetModuleHandle(new PCWSTR(null)),
                        null
                    );

                    if (hWnd == default)
                    {
                        int error = Marshal.GetLastWin32Error();
                        CustomLogger.Error($"CreateWindowEx failed with error: {error}", LOGTAG);
                    }
                    else
                    {
                        CustomLogger.Info($"ヘルパーウィンドウを作成しました: {hWnd}", LOGTAG);

                        // ウィンドウを非表示にする
                        PInvoke.ShowWindow(hWnd, SHOW_WINDOW_CMD.SW_HIDE);
                    }
                }
            }
        }

        /// <summary>
        /// ウィンドウプロシージャ（トレイアイコンからのメッセージを処理）
        /// </summary>
        private LRESULT WindowProc(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam)
        {
            try
            {
                CustomLogger.Info($"WindowProc呼び出し: msg=0x{msg:X}, wParam={wParam.Value}, lParam={lParam.Value}", LOGTAG);

                // トレイアイコンからのメッセージを処理
                if (msg == WM_TRAYICON)
                {
                    int mouseMsg = (int)lParam.Value;
                    CustomLogger.Info($"トレイアイコンメッセージ受信: {mouseMsg} (0x{mouseMsg:X})", LOGTAG);

                    switch (mouseMsg)
                    {
                        case (int)PInvoke.WM_LBUTTONDOWN:
                            CustomLogger.Info("トレイアイコンが左クリックされました", LOGTAG);
                            OnTrayIconLeftClick();
                            break;

                        case (int)PInvoke.WM_RBUTTONDOWN:
                            CustomLogger.Info("トレイアイコンが右クリックされました", LOGTAG);
                            OnTrayIconRightClick();
                            break;
                    }
                    return new LRESULT(0);
                }
                else if (msg == PInvoke.WM_COMMAND)
                {
                    // メニューコマンドの処理
                    uint cmdId = (uint)(wParam.Value);
                    HandleMenuCommand(cmdId);
                    return new LRESULT(0);
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"WindowProc内でエラー: {ex.Message}\n{ex.StackTrace}", LOGTAG);
            }

            // デフォルトのウィンドウプロシージャにメッセージを転送
            return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        /// <summary>
        /// トレイアイコンの左クリック処理
        /// </summary>
        private void OnTrayIconLeftClick()
        {
            CustomLogger.Info("左クリック処理実行", LOGTAG);
            // ここに左クリック時の処理を実装
        }

        /// <summary>
        /// トレイアイコンの右クリック処理
        /// </summary>
        private void OnTrayIconRightClick()
        {
            CustomLogger.Info("右クリック処理実行", LOGTAG);
            ShowContextMenu();
        }

        /// <summary>
        /// コンテキストメニューを表示します
        /// </summary>
        private void ShowContextMenu()
        {
            try
            {
                // ポップアップメニューを作成
                HMENU hMenu = PInvoke.CreatePopupMenu();
                if (hMenu == IntPtr.Zero)
                {
                    CustomLogger.Error("ポップアップメニューの作成に失敗しました", LOGTAG);
                    return;
                }

                // メニューアイテムを追加
                unsafe
                {
                    fixed (char* helloTextPtr = "こんにちは")
                    fixed (char* exitTextPtr = "終了")
                    {
                        // "こんにちは" メニューアイテムを追加
                        BOOL result1 = PInvoke.AppendMenu(
                            hMenu,
                            MENU_ITEM_FLAGS.MF_STRING,
                            IDM_HELLO,
                            helloTextPtr
                        );

                        // セパレータを追加
                        BOOL result2 = PInvoke.AppendMenu(
                            hMenu,
                            MENU_ITEM_FLAGS.MF_SEPARATOR,
                            0,
                            null
                        );

                        // "終了" メニューアイテムを追加
                        BOOL result3 = PInvoke.AppendMenu(
                            hMenu,
                            MENU_ITEM_FLAGS.MF_STRING,
                            IDM_EXIT,
                            exitTextPtr
                        );

                        if (!result1 || !result3)
                        {
                            CustomLogger.Error("メニューアイテムの追加に失敗しました", LOGTAG);
                            PInvoke.DestroyMenu(hMenu);
                            return;
                        }
                    }
                }

                // カーソル位置を取得
                System.Drawing.Point cursorPos;
                if (!PInvoke.GetCursorPos(out cursorPos))
                {
                    CustomLogger.Error("カーソル位置の取得に失敗しました", LOGTAG);
                    PInvoke.DestroyMenu(hMenu);
                    return;
                }

                CustomLogger.Info($"カーソル位置: ({cursorPos.X}, {cursorPos.Y})", LOGTAG);

                // ウィンドウをフォアグラウンドにする（重要：これがないとメニューが正常に動作しない）
                PInvoke.SetForegroundWindow(hWnd);
                unsafe
                {

                    // ポップアップメニューを表示
                    BOOL menuResult = PInvoke.TrackPopupMenu(
                        hMenu,
                        TRACK_POPUP_MENU_FLAGS.TPM_LEFTBUTTON | TRACK_POPUP_MENU_FLAGS.TPM_NONOTIFY,
                        cursorPos.X,
                        cursorPos.Y,
                        0,  // nReserved
                        hWnd,
                        null  // prcRect
                    );
                }


                // メニューを破棄
                PInvoke.DestroyMenu(hMenu);

                // ウィンドウをバックグラウンドに戻す
                PInvoke.PostMessage(hWnd, PInvoke.WM_NULL, 0, 0);
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"コンテキストメニュー表示中にエラー: {ex.Message}\n{ex.StackTrace}", LOGTAG);
            }
        }
        /// <summary>
        /// メニューコマンドの処理
        /// </summary>
        private void HandleMenuCommand(uint cmdId)
        {
            switch (cmdId)
            {
                case IDM_EXIT:
                    CustomLogger.Info("終了コマンドが選択されました", LOGTAG);
                    // 通知アイコンを削除してからアプリケーションを終了
                    RemoveNotificationIcon();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
                    break;
                case IDM_HELLO:
                    CustomLogger.Info("こんにちはコマンドが選択されました", LOGTAG);
                    ShowHelloMessage();
                    break;
                default:
                    CustomLogger.Info($"不明なコマンド: {cmdId}", LOGTAG);
                    break;
            }
        }

        /// <summary>
        /// こんにちはメッセージを表示します
        /// </summary>
        private void ShowHelloMessage()
        {
            try
            {
                unsafe
                {
                    fixed (char* titlePtr = "V-LinkBattle")
                    fixed (char* messagePtr = "こんにちは！\nV-LinkBattleが実行中です。")
                    {
                        PInvoke.MessageBox(
                            HWND.Null,
                            messagePtr,
                            titlePtr,
                            MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONINFORMATION
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"メッセージボックス表示中にエラー: {ex.Message}", LOGTAG);
            }
        }
    }
}