using TechC.VBattle.Core.Managers;
using TechC.VBattle.InGame.Systems;

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
        private BattleJudge battleJudge;

        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
            BattleBus = new BattleEventBus();
            // BattleJudge = new BattleJudge();
        }

        private void Update()
        {
            UpdateState();
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            BattleBus.Clear();
        }


        private void UpdateState()
        {
            switch(inGameState)
            {
                case InGameState.Start:

                    break;
                case InGameState.Battle:
                    
                    break;
                case InGameState.Result:

                    break;
            }
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
