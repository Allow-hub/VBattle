using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Util;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TechC.VBattle.Core.Window
{
    /// <summary>
    /// ウィンドウプロシージャを管理する
    /// </summary>
    public static class WindowClassManager
    {
        private static bool _classRegistered = false;
        private static string _basicClassName = "WindowClass_Basic";
        private static string _imageClassName = "WindowClass_Image";
        private static string _webClassName = "WindowClass_Web";


        // WndProcデリゲート保持（GC防止）
        private static readonly WNDPROC _basicWndProc = BasicWndProc;
        private static readonly WNDPROC _imageWndProc = ImageWndProc;
        private static readonly WNDPROC _webWndProc = WebWndProc;

        /// <summary>
        /// ウィンドウクラスを登録する
        /// </summary>
        public static void RegisterWindowClasses()
        {
            if (_classRegistered)
                return;

            var hInstance = PInvoke.GetModuleHandle((PCWSTR)default);
            RegisterClassEx(_basicClassName, _basicWndProc, hInstance);
            RegisterClassEx(_imageClassName, _imageWndProc, hInstance);
            RegisterClassEx(_webClassName, _webWndProc, hInstance);

            _classRegistered = true;
            CustomLogger.Info("Window classes registered.", LogTagUtil.TagWidnow);
        }

        /// <summary>
        /// ウィンドウクラスを登録解除する
        /// </summary>
        public static void UnregisterWindowClasses()
        {
            if (!_classRegistered)
                return;

            var hInstance = PInvoke.GetModuleHandle((PCWSTR)default);

            unsafe
            {
                fixed (char* basicName = _basicClassName)
                {
                    if (!PInvoke.UnregisterClass(new PCWSTR(basicName), hInstance))
                        CustomLogger.Error($"UnregisterClass failed for {_basicClassName}, error: {Marshal.GetLastWin32Error()}", LogTagUtil.TagWidnow);
                }

                fixed (char* imageName = _imageClassName)
                {
                    if (!PInvoke.UnregisterClass(new PCWSTR(imageName), hInstance))
                        CustomLogger.Error($"UnregisterClass failed for {_imageClassName}, error: {Marshal.GetLastWin32Error()}", LogTagUtil.TagWidnow);
                }

                fixed (char* webName = _webClassName)
                {
                    if (!PInvoke.UnregisterClass(new PCWSTR(webName), hInstance))
                        CustomLogger.Error($"UnregisterClass failed for {_webClassName}, error: {Marshal.GetLastWin32Error()}", LogTagUtil.TagWidnow);
                }
            }

            _classRegistered = false;
            CustomLogger.Info("Window classes unregistered.", LogTagUtil.TagWidnow);
        }

        /// <summary>
        /// ウィンドウクラスを登録する
        /// </summary>
        /// <param name="className">クラス名</param>
        /// <param name="wndProc">ウィンドウプロシージャー</param>
        /// <param name="hInstance">実行中のアプリやDLLを一位に識別するハンドル</param>
        private static void RegisterClassEx(string className, WNDPROC wndProc, HMODULE hInstance)
        {
            unsafe
            {
                fixed (char* cName = className)
                {
                    WNDCLASSEXW wndClass = new()
                    {
                        cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
                        style = 0,
                        lpfnWndProc = wndProc,
                        cbClsExtra = 0,
                        cbWndExtra = 0,
                        hInstance = (HINSTANCE)hInstance,
                        hIcon = default,
                        hCursor = PInvoke.LoadCursor(HINSTANCE.Null, PInvoke.IDC_ARROW),
                        hbrBackground = new Windows.Win32.Graphics.Gdi.HBRUSH((nint)5 + 1),
                        lpszMenuName = null,
                        lpszClassName = new PCWSTR(cName),
                        hIconSm = default,
                    };

                    ushort atom = PInvoke.RegisterClassEx(wndClass);
                    if (atom == 0)
                        CustomLogger.Error($"RegisterClassEx failed for {className}, error: {Marshal.GetLastWin32Error()}", LogTagUtil.TagWidnow);
                    else
                        CustomLogger.Info($"Window class '{className}' registered successfully.", LogTagUtil.TagWidnow);
                }
            }
        }

        /// <summary>
        /// ウィンドウを作成する
        /// </summary>
        /// <param name="className">クラス名</param>
        /// <param name="title">ウィンドウ名</param>
        /// <param name="style">スタイル</param>
        /// <param name="exStyle"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static IntPtr CreateWindow(
            string className,
            string title,
            uint style,
            uint exStyle,
            int x, int y, int width, int height,
            IntPtr parent)
        {
            HWND hwnd;
            unsafe
            {
                fixed (char* cName = className)
                fixed (char* titleName = title)
                {

                    hwnd = PInvoke.CreateWindowEx(
                        (WINDOW_EX_STYLE)exStyle,
                        new PCWSTR(cName),
                        new PCWSTR(titleName),
                        (WINDOW_STYLE)style,
                        x, y, width, height,
                        new HWND(parent),
                        HMENU.Null,
                        PInvoke.GetModuleHandle((PCWSTR)default),
                        null
                    );
                }
            }
            if (hwnd == HWND.Null)
                CustomLogger.Error($"CreateWindowEx failed, error: {Marshal.GetLastWin32Error()}", LogTagUtil.TagWidnow);
            else
                CustomLogger.Info($"CreateWindowEx success: hwnd = {hwnd}", LogTagUtil.TagWidnow);

            return hwnd;
        }

        private static LRESULT BasicWndProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
        {
            switch (msg)
            {
                case PInvoke.WM_PAINT:
                    Windows.Win32.Graphics.Gdi.PAINTSTRUCT ps;
                    Windows.Win32.Graphics.Gdi.HDC hdc = PInvoke.BeginPaint(hwnd, out ps);

                    RECT rcClient;
                    PInvoke.GetClientRect(hwnd, out rcClient);

                    // 背景をクラシック風グレーに塗りつぶし
                    Windows.Win32.Graphics.Gdi.HBRUSH hBrush = PInvoke.CreateSolidBrush(new COLORREF(0x00C0C0C0)); // RGB(192,192,192)
                    unsafe
                    {
                        PInvoke.FillRect(hdc, &rcClient, hBrush);
                    }
                    PInvoke.DeleteObject(hBrush); // 解放忘れ注意

                    // 黒枠を描画
                    Windows.Win32.Graphics.Gdi.HBRUSH hFrameBrush = PInvoke.CreateSolidBrush(new COLORREF(0x000000));
                    unsafe
                    {
                        PInvoke.FrameRect(hdc, &rcClient, hFrameBrush);
                    }
                    PInvoke.DeleteObject(hFrameBrush);

                    // テキスト描画
                    // PInvoke.SetBkMode(hdc, Windows.Win32.Graphics.Gdi.BACKGROUND_MODE.TRANSPARENT);     // TRANSPARENT
                    // PInvoke.SetTextColor(hdc, new COLORREF(0x000000)); // Black
                    // var text = "クラシック風ウィンドウ";
                    // PInvoke.TextOut(hdc, 10, 10, text, text.Length);

                    PInvoke.EndPaint(hwnd, ps);
                    return new LRESULT(0);
                case PInvoke.WM_SIZE:
                    unsafe
                    {
                        PInvoke.InvalidateRect(hwnd, (RECT*)null, true); // 全領域を再描画                    
                    }
                    return new LRESULT(0);
            }

            return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
        }


        private static LRESULT ImageWndProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
        {
            switch (msg)
            {
                case PInvoke.WM_PAINT:
                    break;
            }
            return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
        }
        private static LRESULT WebWndProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
        {
            // switch (msg)
            // {
            //     case PInvoke.WM_PAINT:
            //         // 描画処理
            //         break;
            //     case PInvoke.WM_DESTROY:
            //         break;
            // }

            return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
        }
    }
}