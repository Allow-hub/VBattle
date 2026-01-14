using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// アメのカットイン
    /// </summary>
    public class AmeCutIn : MonoBehaviour, ICutInSequence
    {
        public event Action OnFinished;

        private void OnEnable()
        {
            Play().Forget();
        }

        public async UniTask Play()
        {
            Debug.Log("Ame CutIn Play");
            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            Debug.Log("Ame CutIn Finished");
            OnFinished?.Invoke();
        }
    }
}
