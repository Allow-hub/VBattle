using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// テラミのカットイン
    /// </summary>
    public class TeramiCutIn : MonoBehaviour, ICutInSequence
    {
        public event Action OnFinished;

        public async UniTask Play()
        {
        }
    }
}
