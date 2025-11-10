using System.Collections;
using System.Collections.Generic;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.InGame.Systems;
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
        public BattleEventBus BattleBus { get; private set; }

        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
            BattleBus = new BattleEventBus();
        }
        protected override void OnRelease()
        {
            base.OnRelease();
            BattleBus.ClearAllListeners();
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
