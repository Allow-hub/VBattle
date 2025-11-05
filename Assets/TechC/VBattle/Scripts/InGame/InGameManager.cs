using System.Collections;
using System.Collections.Generic;
using TechC.VBattle.Core.Managers;
using UnityEngine;

namespace TechC.VBattle.InGame
{
    /// <summary>
    /// インゲームの管理クラス
    /// </summary>
    public class InGameManager : Singleton<InGameManager>
    {
        public InGameState InGameState => inGameState;
        private InGameState inGameState;

        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
        }
        protected override void OnRelease()
        {
            base.OnRelease();
        }
    }

    /// <summary>
    /// インゲームのState
    /// </summary>
    public enum InGameState
    {
        Start,
        Battle,
        Result
    }
}
