using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Select.Events;
using UnityEngine;

namespace TechC.VBattle.Select.Core
{
    /// <summary>
    /// キャラクター選択画面の管理とゲーム開始処理を担当
    /// </summary>
    public class CharacterSelectManager : Singleton<CharacterSelectManager>
    {
        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
        }

        private void Start()
        {
            InitializeSelectSystem();
        }

        /// <summary>
        /// キャラクター選択システムの初期化
        /// </summary>
        private void InitializeSelectSystem()
        {
            GameDataBridge.I.SetupPlayer(1, null);
            GameDataBridge.I.SetupPlayer(2, null);

            SelectUIManager.I.EventBus.Subscribe<BothPlayersReadyEvent>(_ => { });
            SelectUIManager.I.OnStartGamePicked += OnGameStartRequested;
        }
        
        private void OnDestroy()
        {
        }

        /// <summary>
        /// ゲーム開始リクエスト処理
        /// </summary>
        private void OnGameStartRequested()
        {
            if (!SelectUIManager.I.HasPicked[0] || !SelectUIManager.I.HasPicked[1]) return;

            var picks = SelectUIManager.I.CurrentPicks;

            // Player 1 の設定
            var player1Data = new GameDataBridge.PlayerSetupData
            {
                PlayerIndex = 1,
                DeviceName = picks[0].inputDevice,
                IsNPC = picks[0].inputDevice == null,
                SelectedCharacter = picks[0].characterData
            };
            GameDataBridge.I.SetupPlayer(1, player1Data);

            // Player 2 の設定
            var player2Data = new GameDataBridge.PlayerSetupData
            {
                PlayerIndex = 2,
                DeviceName = picks[1].inputDevice,
                IsNPC = picks[1].inputDevice == null,
                SelectedCharacter = picks[1].characterData
            };
            GameDataBridge.I.SetupPlayer(2, player2Data);

            SelectUIManager.I.EventBus.Publish(new GameStartEvent
            {
                Player1Character = picks[0].characterData,
                Player2Character = picks[1].characterData,
                Player1Device = picks[0].inputDevice,
                Player2Device = picks[1].inputDevice,
                IsPlayer2Npc = picks[1].inputDevice == null
            });

        }
    }
}