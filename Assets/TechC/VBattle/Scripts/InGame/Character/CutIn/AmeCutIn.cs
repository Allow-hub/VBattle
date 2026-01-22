using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Window;
using UnityEngine;

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

            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            OnFinished?.Invoke();
        }
    }
}
