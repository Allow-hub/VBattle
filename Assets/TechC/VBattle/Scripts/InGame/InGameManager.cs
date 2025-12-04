using System;
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
        [SerializeField] private Vector3 p1Rot;
        [SerializeField] private Vector3 p2Rot;
        [SerializeField] private Vector3 p1Pos;
        [SerializeField] private Vector3 p2Pos;
        
        [SerializeField] private GameObject ameObj;
        public InGameState InGameState => inGameState;
        private InGameState inGameState;
        public BattleEventBus BattleBus { get; private set; }
        private BattleJudge battleJudge;
        private HitStopManager hitStopManager;//イベントを使用しているので保持しておく必要がある
        private bool isPaused = false;          // ポーズ状態フラグ
        public bool IsPaused => isPaused;       // 読み取り専用プロパティ
        public Func<bool> GetPauseStateFunc => () => isPaused;  // Funcデリゲート
        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
            BattleBus = new BattleEventBus();
            hitStopManager = new HitStopManager(BattleBus);
            if (isDebug)
            {
                var p1 = Instantiate(ameObj,p1Pos,Quaternion.Euler(p1Rot)).GetComponent<Character.CharacterController>();
                var p2 = Instantiate(ameObj,p2Pos,Quaternion.Euler(p2Rot)).GetComponent<Character.CharacterController>();

                p1.Init(1,Keyboard.current,false);
                p2.Init(2,Keyboard.current,false);
                p2.GetComponent<PlayerInput>().enabled = false;
                
                battleJudge = new BattleJudge(p1,p2,BattleBus);
            }
            else
            {
                var p1 = Instantiate(GameDataBridge.I.Player_1Setup.SelectedCharacter.CharaPrefab, p1Pos, Quaternion.Euler(p1Rot)).GetComponent<Character.CharacterController>();
                var p2 = Instantiate(GameDataBridge.I.Player_2Setup.SelectedCharacter.CharaPrefab,p2Pos,Quaternion.Euler(p2Rot)).GetComponent<Character.CharacterController>();

                p1.Init(GameDataBridge.I.Player_1Setup.PlayerIndex, GameDataBridge.I.Player_1Setup.DeviceName, GameDataBridge.I.Player_1Setup.IsNPC);
                p2.Init(GameDataBridge.I.Player_2Setup.PlayerIndex, GameDataBridge.I.Player_2Setup.DeviceName, GameDataBridge.I.Player_2Setup.IsNPC);
                Debug.Log($"{GameDataBridge.I.Player_1Setup.PlayerIndex},{GameDataBridge.I.Player_2Setup.PlayerIndex}");
            }
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
