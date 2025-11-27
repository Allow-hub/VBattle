using TechC.VBattle.Core.Managers;
using TechC.VBattle.InGame.Systems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC.VBattle.InGame
{
    /// <summary>
    /// インゲームの管理クラス
    /// </summary>
    public class InGameManager : Singleton<InGameManager>
    {
        [SerializeField] private bool isDebug = true;
        [SerializeField] private GameObject ameObj;
        public InGameState InGameState => inGameState;
        private InGameState inGameState;
        public BattleEventBus BattleBus { get; private set; }
        private BattleJudge battleJudge;

        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
            BattleBus = new BattleEventBus();
            if (isDebug)
            {
                var p1Pos = new Vector3(0,0,-6);
                var p2Pos = new Vector3(2,0,-6);
                var p1 = Instantiate(ameObj,p1Pos,Quaternion.identity).GetComponent<Character.CharacterController>();
                var p2 = Instantiate(ameObj,p2Pos,Quaternion.identity).GetComponent<Character.CharacterController>();

                p1.Initialize(1,Keyboard.current,false);
                p2.Initialize(2,Keyboard.current,false);
                p2.GetComponent<PlayerInput>().enabled = false;
                
                battleJudge = new BattleJudge(p1,p2,BattleBus);                
            }
            else
                Debug.LogError($"まだDebug状態しか対応していません");
        }

        private void Update()
        {
            UpdateState();
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            battleJudge?.Dispose();
            BattleBus?.Clear();
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
