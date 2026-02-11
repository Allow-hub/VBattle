using Cysharp.Threading.Tasks;
using TechC.VBattle.Core;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Select.Events;

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
            GameDataBridge.I.SetupPlayer(PlayerConstants.PLAYER_1_ID, null);
            GameDataBridge.I.SetupPlayer(PlayerConstants.PLAYER_2_ID, null);
            SelectUIManager.I.EventBus.Subscribe<StartGameRequestedEvent>(OnStartGameRequested);
        }

        private void OnDestroy()
        {
            if (SelectUIManager.I != null)
                SelectUIManager.I.EventBus.Unsubscribe<StartGameRequestedEvent>(OnStartGameRequested);
        }


        /// <summary>
        /// ゲーム開始リクエスト処理
        /// </summary>
        private void OnStartGameRequested(StartGameRequestedEvent e)
        {
            if (!SelectUIManager.I.HasPicked[PlayerConstants.PLAYER_1_INDEX] || !SelectUIManager.I.HasPicked[PlayerConstants.PLAYER_2_INDEX]) return;

            var picks = SelectUIManager.I.CurrentPicks;

            var player1Data = new GameDataBridge.PlayerSetupData
            {
                PlayerIndex = PlayerConstants.PLAYER_1_ID,
                DeviceName = picks[PlayerConstants.PLAYER_1_INDEX].isNpc ? null : picks[PlayerConstants.PLAYER_1_INDEX].inputDevice,
                IsNPC = picks[PlayerConstants.PLAYER_1_INDEX].isNpc,  // 明示的なフラグを使用
                SelectedCharacter = picks[PlayerConstants.PLAYER_1_INDEX].characterData
            };
            GameDataBridge.I.SetupPlayer(PlayerConstants.PLAYER_1_ID, player1Data);

            var player2Data = new GameDataBridge.PlayerSetupData
            {
                PlayerIndex = PlayerConstants.PLAYER_2_ID,
                DeviceName = picks[PlayerConstants.PLAYER_2_INDEX].isNpc ? null : picks[PlayerConstants.PLAYER_2_INDEX].inputDevice,
                IsNPC = picks[PlayerConstants.PLAYER_2_INDEX].isNpc,  // 明示的なフラグを使用
                SelectedCharacter = picks[PlayerConstants.PLAYER_2_INDEX].characterData
            };
            GameDataBridge.I.SetupPlayer(PlayerConstants.PLAYER_2_ID, player2Data);

            SelectUIManager.I.EventBus.Publish(new GameStartEvent
            {
                Player1Character = picks[PlayerConstants.PLAYER_1_INDEX].characterData,
                Player2Character = picks[PlayerConstants.PLAYER_2_INDEX].characterData,
                Player1Device = picks[PlayerConstants.PLAYER_1_INDEX].inputDevice,
                Player2Device = picks[PlayerConstants.PLAYER_2_INDEX].inputDevice,
                IsPlayer2Npc = picks[PlayerConstants.PLAYER_2_INDEX].isNpc
            });

            SceneLoader.I.LoadBattleSceneAsync().Forget();
        }
    }
}