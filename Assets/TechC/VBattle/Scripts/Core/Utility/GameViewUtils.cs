using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using Windows.Win32.Foundation;
using TechC.VBattle.Core.Extensions;

namespace TechC.VBattle.Core.Util
{
    /// <summary>
    /// ゲームビュー操作のUtil
    /// </summary>
    public static class GameViewUtils
    {
        private static Type gameViewType;
        private static PropertyInfo targetInViewProperty;
        private static PropertyInfo positionProperty;
        private static MethodInfo getMainGameViewMethod;

        /// <summary>
        /// 静的コンストラクタ: リフレクション用の型とプロパティを初期化
        /// </summary>
        static GameViewUtils()
        {
#if UNITY_EDITOR
            try
            {
                var assembly = typeof(EditorWindow).Assembly;
                gameViewType = assembly.GetType("UnityEditor.GameView");

                if (gameViewType != null)
                {
                    // targetInViewプロパティを取得
                    targetInViewProperty = gameViewType.GetProperty("targetInView",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    // positionプロパティを取得
                    positionProperty = typeof(EditorWindow).GetProperty("position",
                        BindingFlags.Public | BindingFlags.Instance);

                    // PlayModeViewクラスのGetMainPlayModeViewメソッドを取得
                    var playModeViewType = assembly.GetType("UnityEditor.PlayModeView");
                    if (playModeViewType != null)
                    {
                        getMainGameViewMethod = playModeViewType.GetMethod("GetMainPlayModeView",
                            BindingFlags.NonPublic | BindingFlags.Static);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"GameViewUtils初期化エラー: {e.Message}");
            }
#endif
        }

        /// <summary>
        /// メインGameViewのEditorWindowインスタンスを取得
        /// </summary>
        /// <returns>GameViewのEditorWindowインスタンス、取得できない場合はnull</returns>
        public static EditorWindow GetGameViewWindow()
        {
#if UNITY_EDITOR
            try
            {
                // まずGetMainPlayModeViewメソッドを試す
                if (getMainGameViewMethod != null)
                {
                    var gameView = getMainGameViewMethod.Invoke(null, null) as EditorWindow;
                    if (gameView != null)
                        return gameView;
                }

                // フォールバック: Resources.FindObjectsOfTypeAllで検索
                if (gameViewType != null)
                {
                    var windows = Resources.FindObjectsOfTypeAll(gameViewType);
                    if (windows != null && windows.Length > 0)
                        return windows[0] as EditorWindow;
                }

                if (gameViewType != null)
                    return EditorWindow.GetWindow(gameViewType, false, null, false);
            }
            catch (Exception e)
            {
                Debug.LogError($"GameViewWindow取得エラー: {e.Message}");
            }
#endif
            return null;
        }

        /// <summary>
        /// ゲームビューのスクリーンRectを取得（GameView内の相対座標）
        /// </summary>
        /// <returns>ゲーム画面のRect</returns>
        public static Rect GetGameViewScreenRect()
        {
#if UNITY_EDITOR
            try
            {
                var gameView = GetGameViewWindow();
                if (gameView != null && targetInViewProperty != null)
                {
                    var targetRect = (Rect)targetInViewProperty.GetValue(gameView);
                    return targetRect;
                }
                else
                    Debug.LogWarning("GameViewまたはtargetInViewプロパティが取得できませんでした");
            }
            catch (Exception e)
            {
                Debug.LogError($"GameViewスクリーンRect取得エラー: {e.Message}");
            }

            // エラー時はフォールバック
            return new Rect(0, 0, Screen.width, Screen.height);
#else
            // 実機ではフルスクリーンを想定
            return new Rect(
                Screen.mainWindowPosition.x,
                Screen.mainWindowPosition.y,
                Screen.width,
                Screen.height
            );
#endif
        }

        /// <summary>
        /// エディタのスクリーン座標系でのゲームビューのRect取得
        /// （エディタウィンドウ全体での絶対座標）
        /// </summary>
        /// <returns>エディタスクリーン座標系でのゲーム画面Rect</returns>
        public static Rect GetGameViewScreenRectInEditorSpace()
        {
#if UNITY_EDITOR
            try
            {
                var gameView = GetGameViewWindow();
                if (gameView == null)
                {
                    CustomLogger.Warning("GameViewウィンドウが見つかりません", LogTagUtil.TagWidnow);
                    return Rect.zero;
                }

                // GameViewウィンドウの位置を取得
                var windowPosition = gameView.position;

                // GameView内でのゲーム画面の相対位置を取得
                var gameRect = GetGameViewScreenRect();

                // 絶対座標に変換
                return new Rect(
                    windowPosition.x + gameRect.x,
                    windowPosition.y + gameRect.y,
                    gameRect.width,
                    gameRect.height
                );
            }
            catch (Exception e)
            {
                CustomLogger.Error($"エディタ空間でのGameViewRect取得エラー: {e.Message}", LogTagUtil.TagWidnow);
                return Rect.zero;
            }
#else
            return new Rect(
                Screen.mainWindowPosition.x,
                Screen.mainWindowPosition.y,
                Screen.width,
                Screen.height
            );
#endif
        }

        /// <summary>
        /// GameViewウィンドウ全体の位置とサイズを取得
        /// </summary>
        /// <returns>GameViewウィンドウのRect</returns>
        public static Rect GetGameViewWindowRect()
        {
#if UNITY_EDITOR
            try
            {
                var gameView = GetGameViewWindow();
                if (gameView != null)
                    return gameView.position;
            }
            catch (Exception e)
            {
                Debug.LogError($"GameViewウィンドウRect取得エラー: {e.Message}");
            }
            return Rect.zero;
#else
            return GetGameViewScreenRect();
#endif
        }

        /// <summary>
        /// RectをWin32のRECTに変換
        /// </summary>
        /// <param name="unityRect">UnityのRect</param>
        /// <returns>Win32のRECT構造体</returns>
        public static RECT ToWin32Rect(Rect unityRect)
        {
            int left = Mathf.FloorToInt(unityRect.x);
            int top = Mathf.FloorToInt(unityRect.y);
            int right = Mathf.CeilToInt(unityRect.x + unityRect.width);
            int bottom = Mathf.CeilToInt(unityRect.y + unityRect.height);

            return new RECT
            {
                left = left,
                top = top,
                right = right,
                bottom = bottom
            };
        }

#if UNITY_EDITOR
        /// <summary>
        /// デバッグ情報を出力
        /// </summary>
        [MenuItem("Tools/GameView/Debug Info")]
        public static void DebugInfo()
        {
            Debug.Log("=== GameView Debug Info ===");

            var gameRect = GetGameViewScreenRect();
            Debug.Log($"GameView Screen Rect (相対): {gameRect}");

            var editorRect = GetGameViewScreenRectInEditorSpace();
            Debug.Log($"GameView Screen Rect (絶対): {editorRect}");

            var windowRect = GetGameViewWindowRect();
            Debug.Log($"GameView Window Rect: {windowRect}");

            var win32Rect = ToWin32Rect(editorRect);
            Debug.Log($"Win32 RECT: L={win32Rect.left} T={win32Rect.top} R={win32Rect.right} B={win32Rect.bottom}");
        }
#endif

    }
}