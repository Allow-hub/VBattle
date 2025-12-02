using System.Collections;
using System.Collections.Generic;
using TechC.VBattle.Core.Window;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.UI;
using System;

namespace TechC.VBattle.Select
{
    /// <summary>
    /// ゲームパッド用のポインター表示とクリック処理
    /// </summary>
    public class GamepadPointer : MonoBehaviour
    {
        [SerializeField] private List<Sprite> pointerSprite = new List<Sprite>();
        [SerializeField] private float cursorSpeed = 800f;
        [SerializeField] private Vector2 cursorSize = new Vector2(64, 64);
        [SerializeField] private Canvas targetCanvas; // Unity UI 用

        private Dictionary<InputDevice, NativeWindow> nativeWindows = new Dictionary<InputDevice, NativeWindow>();
        private Dictionary<InputDevice, Vector2> cursorPositions = new Dictionary<InputDevice, Vector2>();
        private IDisposable currentListener;

        private void OnEnable()
        {
            StartCoroutine(WaitForNextDevice());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            foreach (var w in nativeWindows)
            {
                WindowFactory.I.ReturnWindow(w.Value);
            }
            currentListener?.Dispose();
            currentListener = null;

            nativeWindows.Clear();
            cursorPositions.Clear();
        }

        private void Update()
        {
            foreach (var pair in nativeWindows)
            {
                var device = pair.Key;
                var window = pair.Value;

                if (device is Gamepad gamepad)
                {
                    // 左スティックで移動
                    Vector2 stick = gamepad.leftStick.ReadValue();

                    if (!cursorPositions.ContainsKey(device))
                        cursorPositions[device] = new Vector2(Screen.width / 2, Screen.height / 2);

                    Vector2 pos = cursorPositions[device];
                    pos += stick * cursorSpeed * Time.deltaTime;
                    pos.x = Mathf.Clamp(pos.x, 0, Screen.width - 1);
                    pos.y = Mathf.Clamp(pos.y, 0, Screen.height - 1);
                    cursorPositions[device] = pos;

                    int winX = (int)pos.x;
                    int winY = (int)(Screen.height - pos.y);
                    WindowUtility.MoveWindow((HWND)window.Hwnd, winX, winY);

                    // ✕ボタンでクリック
                    if (gamepad.buttonSouth.wasPressedThisFrame)
                    {
                        SimulateClick(winX, winY);        // 外部ウィンドウ用
                        SendUnityUIClick(pos, gamepad);   // Unity UI 用（Gamepadデバイス付き）
                    }
                }
            }
        }

        /// <summary>
        /// 外部ウィンドウへ擬似クリック送信
        /// </summary>
        private void SimulateClick(int x, int y)
        {
            // 現在のマウス位置を保持
            PInvoke.GetCursorPos(out var originalPos);

            INPUT[] inputs = new INPUT[3];
            int screenW = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN);
            int screenH = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN);

            inputs[0].type = INPUT_TYPE.INPUT_MOUSE;
            inputs[0].Anonymous.mi = new MOUSEINPUT
            {
                dx = x * 65535 / screenW,
                dy = y * 65535 / screenH,
                dwFlags = MOUSE_EVENT_FLAGS.MOUSEEVENTF_MOVE | MOUSE_EVENT_FLAGS.MOUSEEVENTF_ABSOLUTE,
                mouseData = 0,
                time = 0,
                dwExtraInfo = UIntPtr.Zero
            };
            inputs[1].type = INPUT_TYPE.INPUT_MOUSE;
            inputs[1].Anonymous.mi = new MOUSEINPUT
            {
                dwFlags = MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTDOWN
            };
            inputs[2].type = INPUT_TYPE.INPUT_MOUSE;
            inputs[2].Anonymous.mi = new MOUSEINPUT
            {
                dwFlags = MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTUP
            };

            ReadOnlySpan<INPUT> span = new(inputs);
            PInvoke.SendInput(span, System.Runtime.InteropServices.Marshal.SizeOf<INPUT>());

            // 元のマウス位置に戻す
            PInvoke.SetCursorPos(originalPos.X, originalPos.Y);
        }

        /// <summary>
        /// Unity UIへ擬似クリック送信（Gamepadデバイス付き）
        /// </summary>
        private void SendUnityUIClick(Vector2 screenPos, Gamepad gamepad)
        {
            if (!targetCanvas) return;

            var pointerData = new ExtendedPointerEventData(EventSystem.current)
            {
                device = gamepad,
                position = screenPos,
                button = PointerEventData.InputButton.Left,
                clickCount = 1,
                eligibleForClick = true
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                ExecuteEvents.Execute(result.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
            }
        }

        private IEnumerator WaitForNextDevice()
        {
            yield return null;

            currentListener = InputSystem.onAnyButtonPress.CallOnce(ctrl =>
            {
                if (!this) return;

                var device = ctrl.device;
                if (device is Mouse || device is Keyboard)
                {
                    StartCoroutine(WaitForNextDevice());
                    return;
                }

                if (nativeWindows.Count >= pointerSprite.Count)
                {
                    return;
                }

                if (nativeWindows.ContainsKey(device))
                {
                    StartCoroutine(WaitForNextDevice());
                    return;
                }

                // カーソルウィンドウ生成
                var w = WindowFactory.I.GetWindow(WindowFactory.WindowType.Image);
                int style = PInvoke.GetWindowLong((HWND)w.Hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
                style &= ~(int)WINDOW_STYLE.WS_CAPTION;
                style &= ~(int)WINDOW_STYLE.WS_THICKFRAME;
                style |= unchecked((int)WINDOW_STYLE.WS_POPUP);
                PInvoke.SetWindowLong((HWND)w.Hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, style);

                int exStyle = PInvoke.GetWindowLong((HWND)w.Hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
                exStyle |= (int)WINDOW_EX_STYLE.WS_EX_LAYERED;
                PInvoke.SetWindowLong((HWND)w.Hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, exStyle);

                WindowUtility.ResizeWindow((HWND)w.Hwnd, (int)cursorSize.x, (int)cursorSize.y);

                var spriteToUse = pointerSprite[nativeWindows.Count];
                if (w is ImageWindow imageWindow)
                    imageWindow?.SetTextureToBitmap(spriteToUse.texture);

                Vector2 startPos = new Vector2(Screen.width / 2, Screen.height / 2);
                cursorPositions[device] = startPos;

                int startX = (int)startPos.x;
                int startY = (int)(Screen.height - startPos.y);
                WindowUtility.MoveWindow((HWND)w.Hwnd, startX, startY);

                nativeWindows[device] = w;

                StartCoroutine(WaitForNextDevice());
            });
        }
    }
}