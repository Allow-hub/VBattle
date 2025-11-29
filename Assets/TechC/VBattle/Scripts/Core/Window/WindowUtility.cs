using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TechC.VBattle.Core.Window
{
    /// <summary>
    /// Windowsのウィンドウ操作を簡単に行うためのユーティリティクラス
    /// </summary>
    public static class WindowUtility
    {
        public static string WINDOWLOGTAG = "window";
        #region ウィンドウ作成・取得

        /// <summary>
        /// Unityのゲームビューの矩形を取得
        /// </summary>
        /// <returns>ゲームビューの矩形</returns>
        public static RECT GetUnityGameViewRect()
        {
#if UNITY_EDITOR
            var gameView = FindWindowWithTitleSubstring("Game");
            if (gameView != IntPtr.Zero)
            {
                return GetWindowRect(gameView);
            }
            else
            {
                // 画面全体の解像度をRECTで返す
                return new RECT
                {
                    left = 0,
                    top = 0,
                    right = Screen.width,
                    bottom = Screen.height
                };
            }
#else
    RECT rect;
    GetClientRect(GetUnityWindowHandle(), out rect);
    return rect;
#endif
        }
        public static HWND FindWindowWithTitleSubstring(string substring)
        {
            HWND result = default;

            PInvoke.EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd))
                    return true;

                int length = PInvoke.GetWindowTextLength(hWnd);
                if (length == 0)
                    return true;

                Span<char> buffer = stackalloc char[length + 1];
                int copied = GetWindowText(hWnd, buffer);
                string title = copied > 0 ? new string(buffer.Slice(0, copied)) : "";

                if (title.Contains(substring, StringComparison.OrdinalIgnoreCase))
                {
                    result = hWnd;
                    return false; // 見つかったので列挙停止
                }

                return true; // 続行
            }, IntPtr.Zero);

            return result;
        }

        private static int GetWindowText(HWND hWnd, Span<char> buffer)
        {
            unsafe
            {
                fixed (char* ptr = buffer)
                {
                    return PInvoke.GetWindowText(hWnd, ptr, buffer.Length);
                }
            }
        }

        #endregion

        #region ウィンドウ位置・サイズ操作

        /// <summary>
        /// ウィンドウを指定した位置に移動
        /// </summary>
        /// <param name="hwnd">ウィンドウハンドル</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <returns>成功した場合true</returns>
        public static bool MoveWindow(HWND hwnd, int x, int y)
        {
            var rect = GetWindowRect(hwnd);
            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            return PInvoke.MoveWindow(hwnd, x, y, width, height, true);
        }
        #endregion

        #region ウィンドウ状態操作

        /// <summary>
        /// ウィンドウのサイズを変更
        /// </summary>
        /// <param name="hwnd">ウィンドウハンドル</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <returns>成功した場合true</returns>
        public static bool ResizeWindow(HWND hwnd, int width, int height)
        {
            var rect = GetWindowRect(hwnd);
            return PInvoke.MoveWindow(hwnd, rect.left, rect.top, width, height, true);
        }

        #endregion

        #region ウィンドウ情報取得

        /// <summary>
        /// ウィンドウの矩形を取得
        /// </summary>
        /// <param name="hwnd">ウィンドウハンドル</param>
        /// <returns>ウィンドウの矩形</returns>
        public static RECT GetWindowRect(HWND hwnd)
        {
            RECT rect;
            PInvoke.GetWindowRect(hwnd, out rect);
            return rect;
        }
        /// <summary>
        /// ウィンドウが表示されているかどうかを確認
        /// </summary>
        /// <param name="hwnd">ウィンドウハンドル</param>
        /// <returns>表示されている場合true</returns>
        public static bool IsWindowVisible(HWND hwnd)
        {
            return PInvoke.IsWindowVisible(hwnd);
        }

        /// <summary>
        /// ウィンドウハンドルが有効かどうかを確認
        /// </summary>
        /// <param name="hwnd">ウィンドウハンドル</param>
        /// <returns>有効な場合true</returns>
        public static bool IsValidWindow(HWND hwnd)
        {
            return !hwnd.IsNull && PInvoke.IsWindow(hwnd);
        }
        #endregion

        #region WindowManager用の追加メソッド
        /// <summary>
        /// ウィンドウの表示状態を設定
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="showCommand">表示コマンド</param>
        /// <returns>成功した場合true</returns>
        public static bool SetWindowVisibility(IntPtr hWnd, int showCommand)
        {
            SetWindowTheme((HWND)hWnd, "", "");
            return PInvoke.ShowWindow(new HWND(hWnd), (SHOW_WINDOW_CMD)showCommand);
        }

        public static bool SetWindowPos(HWND hwnd, HWND insertAfter, int x, int y,
            int width, int height, SET_WINDOW_POS_FLAGS flags)
        {
            return PInvoke.SetWindowPos(hwnd, insertAfter, x, y, width, height, flags);
        }

        /// <summary>
        /// ウィンドウハンドルを破棄
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>成功した場合true</returns>
        public static bool DestroyWindowHandle(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                Debug.LogWarning("DestroyWindowHandle: hwnd is null.");
                return false;
            }

            if (!PInvoke.IsWindow((HWND)hwnd))
            {
                Debug.LogWarning("DestroyWindowHandle: hwnd is not a valid window.");
                return false;
            }

            if (!PInvoke.DestroyWindow((HWND)hwnd))
            {
                int error = Marshal.GetLastWin32Error();
                Debug.LogError($"DestroyWindowHandle failed with error code {error}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// ウィンドウの再描画設定
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="redraw">再描画するかどうか</param>
        public static void SetRedraw(IntPtr hWnd, bool redraw)
        {
            PInvoke.SendMessage(new HWND(hWnd), PInvoke.WM_SETREDRAW,
                new WPARAM((nuint)(redraw ? 1 : 0)), new LPARAM(0));

            if (redraw)
            {
                PInvoke.InvalidateRect(new HWND(hWnd), new RECT(), true);
                PInvoke.UpdateWindow(new HWND(hWnd));
            }
        }

        public static void SetWindowTheme(HWND hwnd, string themeName, string pszSubIdList = null)
        {
            if (hwnd.IsNull)
            {
                Debug.LogWarning("SetWindowTheme: hwnd is null.");
                return;
            }

            if (!PInvoke.IsWindow(hwnd))
            {
                Debug.LogWarning("SetWindowTheme: hwnd is not a valid window.");
                return;
            }

            PInvoke.SetWindowTheme(hwnd, themeName, pszSubIdList);
        }

        #endregion

        #region アニメーション用メソッド
        public static async UniTask MoveWindowToTargetAsync(
            NativeWindow nativeWindow,
            int targetX,
            int targetY,
            float moveSpeedPerFrame = 10f,
            int intervalMs = 16,
            Texture2D texture = null)
        {
            if (!IsValidWindow(new HWND(nativeWindow.Hwnd))) return;

            HWND hWnd= HWND.Null;
            if (nativeWindow is WebWindow webWindow)
                hWnd = webWindow.WebWindowHwnd;
            else
                hWnd = (HWND)nativeWindow.Hwnd;

            while (true)
            {
                var rect = GetWindowRect(hWnd);
                Vector2 currentPos = new Vector2(rect.left, rect.top);
                Vector2 targetPos = new Vector2(targetX, targetY);
                Vector2 toTarget = targetPos - currentPos;
                float distance = toTarget.magnitude;

                if (distance < moveSpeedPerFrame)
                {
                    MoveWindow(hWnd, targetX, targetY);
                    break;
                }

                Vector2 direction = toTarget.normalized;
                Vector2 newPos = currentPos + direction * moveSpeedPerFrame;

                MoveWindow(hWnd, Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.y));

                if (nativeWindow is ImageWindow imageWindow)
                    imageWindow.SetImage(texture);

                await UniTask.Delay(intervalMs);
            }
        }
        public static async UniTask MoveWindowInDirectionAsync(
            NativeWindow nativeWindow,
            Vector2 direction,
            float moveSpeedPerFrame = 10f,
            int intervalMs = 16,
            Texture2D texture = null,
            float durationSeconds = -1f) // duration < 0 で無限に動く
        {
            if (!IsValidWindow(new HWND(nativeWindow.Hwnd))) return;

            HWND hWnd = nativeWindow is WebWindow webWindow
                ? webWindow.WebWindowHwnd
                : (HWND)nativeWindow.Hwnd;

            direction = direction.normalized;

            float elapsed = 0f;
            while (durationSeconds < 0f || elapsed < durationSeconds)
            {
                var rect = GetWindowRect(hWnd);
                Vector2 currentPos = new Vector2(rect.left, rect.top);
                Vector2 newPos = currentPos + direction * moveSpeedPerFrame;

                MoveWindow(hWnd, Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.y));

                if (nativeWindow is ImageWindow imageWindow)
                    imageWindow.SetImage(texture);

                await UniTask.Delay(intervalMs);
                elapsed += intervalMs / 1000f;
            }
        }


        /// <summary>
        /// ウィンドウを目標位置にアニメーションで移動（Lerp補間）
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="targetX">目標X座標</param>
        /// <param name="targetY">目標Y座標</param>
        /// <param name="lerpSpeed">補間速度（0.01～0.5程度）</param>
        /// <returns>目標位置に到達した場合true</returns>
        public static bool MoveWindowToTargetPosition(IntPtr hWnd, int targetX, int targetY, float lerpSpeed = 0.1f)
        {
            if (!IsValidWindow(new HWND(hWnd))) return false;

            var rect = GetWindowRect(new HWND(hWnd));
            var currentX = rect.left;
            var currentY = rect.top;

            Vector2 currentPos = new Vector2(currentX, currentY);
            Vector2 targetPos = new Vector2(targetX, targetY);

            // Lerp補間で移動
            Vector2 newPos = Vector2.Lerp(currentPos, targetPos, lerpSpeed);

            bool reached = Vector2.Distance(newPos, targetPos) < 1f;

            if (reached)
            {
                // 最終位置に強制セット
                MoveWindow(new HWND(hWnd), targetX, targetY);
            }
            else
            {
                MoveWindow(new HWND(hWnd), Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.y));
            }

            return reached;
        }
        /// <summary>
        /// ウィンドウサイズをアニメーションで変更（Lerp補間）
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="targetWidth">目標の幅</param>
        /// <param name="targetHeight">目標の高さ</param>
        /// <param name="lerpSpeed">補間スピード（0.01～0.5推奨）</param>
        /// <param name="intervalMs">1ステップの間隔（デフォルト16ms = 約60fps）</param>
        public static async UniTask AnimateResizeWindowAsync(IntPtr hWnd, int targetWidth, int targetHeight, float lerpSpeed = 0.1f, int intervalMs = 16)
        {
            if (!IsValidWindow(new HWND(hWnd))) return;

            HWND hwnd = new HWND(hWnd);

            while (true)
            {
                var rect = GetWindowRect(hwnd);
                int currentWidth = rect.right - rect.left;
                int currentHeight = rect.bottom - rect.top;

                Vector2 currentSize = new Vector2(currentWidth, currentHeight);
                Vector2 targetSize = new Vector2(targetWidth, targetHeight);

                Vector2 newSize = Vector2.Lerp(currentSize, targetSize, lerpSpeed);
                bool reached = Vector2.Distance(newSize, targetSize) < 1f;

                if (reached)
                {
                    ResizeWindow(hwnd, targetWidth, targetHeight);
                    break;
                }
                else
                {
                    ResizeWindow(hwnd, Mathf.RoundToInt(newSize.x), Mathf.RoundToInt(newSize.y));
                }

                await UniTask.Delay(intervalMs);
            }
        }
        /// <summary>
        /// ウィンドウサイズをアニメーションで変更（Lerpによる滑らか補間）
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="targetWidth">目標幅</param>
        /// <param name="targetHeight">目標高さ</param>
        /// <param name="lerpSpeed">補間速度（0.01～0.5程度）</param>
        /// <returns>目標サイズに到達した場合true</returns>
        public static bool AnimateResizeWindow(IntPtr hWnd, int targetWidth, int targetHeight, float lerpSpeed = 0.1f)
        {
            if (!IsValidWindow(new HWND(hWnd))) return false;

            var rect = GetWindowRect(new HWND(hWnd));
            int currentWidth = rect.right - rect.left;
            int currentHeight = rect.bottom - rect.top;

            Vector2 currentSize = new Vector2(currentWidth, currentHeight);
            Vector2 targetSize = new Vector2(targetWidth, targetHeight);

            Vector2 newSize = Vector2.Lerp(currentSize, targetSize, lerpSpeed);

            bool reached = Vector2.Distance(newSize, targetSize) < 1f;

            if (reached)
            {
                // 最終サイズに強制セット
                ResizeWindow(new HWND(hWnd), targetWidth, targetHeight);
            }
            else
            {
                ResizeWindow(new HWND(hWnd), Mathf.RoundToInt(newSize.x), Mathf.RoundToInt(newSize.y));
            }

            return reached;
        }
        #endregion
    }
}