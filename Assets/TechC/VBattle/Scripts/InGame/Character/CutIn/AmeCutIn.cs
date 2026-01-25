using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Window;
using UnityEngine;
using Windows.Win32.Foundation;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// アメのカットイン
    /// </summary>
    public class AmeCutIn : MonoBehaviour, ICutInSequence
    {
        [SerializeField] private Sprite tex;
        [SerializeField] private float intervalPerWindow = 0.01f;
        public event Action OnFinished;

        private void OnEnable()
        {
            Play().Forget();
        }

        public async UniTask Play()
        {
            var w = WindowFactory.I.GetWindow(WindowFactory.WindowType.Image);
            WindowUtility.MoveWindow((HWND)w.Hwnd, 0, Screen.height);
            WindowUtility.ResizeWindow((HWND)w.Hwnd, Screen.width, Screen.height);

            if (w is ImageWindow imageWindow)
                imageWindow.SetTextureToBitmap(tex.texture);
            // 画面外から画面内(0, 0)にアニメーション移動
            await WindowUtility.MoveWindowToTargetAsync(w, 0, 0, moveSpeedPerFrame: 30, intervalMs: 16);
            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            w.Hide();
            WindowFactory.I.ReturnWindow(w);
            await WindowManager.I.DismissPopupWindowsAsync(intervalPerWindow);
            OnFinished?.Invoke();
        }
    }
}
