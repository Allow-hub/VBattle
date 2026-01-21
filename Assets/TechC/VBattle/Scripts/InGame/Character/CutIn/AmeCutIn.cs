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
        public event Action OnFinished;

        private void OnEnable()
        {
            Play().Forget();
        }

        public async UniTask Play()
        {
            var w = WindowFactory.I.GetWindow(WindowFactory.WindowType.Image);
            WindowUtility.ResizeWindow((HWND)w.Hwnd, Screen.width, Screen.height);

            if (w is ImageWindow imageWindow)
            {
                imageWindow.SetTextureToBitmap(tex.texture);
                WindowUtility.MoveWindow((HWND)w.Hwnd, 0, -Screen.height);
            }
            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            OnFinished?.Invoke();
        }
    }
}
