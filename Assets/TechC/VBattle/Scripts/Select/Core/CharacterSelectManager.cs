using Cysharp.Threading.Tasks;
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
        
        private const int PLAYER_1_INDEX = 0;
        private const int PLAYER_2_INDEX = 1;
        private const int PLAYER_1_ID = 1;
        private const int PLAYER_2_ID = 2;

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
            SelectUIManager.I.OnStartGamePicked += OnGameStartRequested;
        }


        /// <summary>
        /// ゲーム開始リクエスト処理
        /// </summary>
        private void OnGameStartRequested()
        {
            if (!SelectUIManager.I.HasPicked[PLAYER_1_INDEX] || !SelectUIManager.I.HasPicked[PLAYER_2_INDEX]) return;

            var picks = SelectUIManager.I.CurrentPicks;

            var player1Data = new GameDataBridge.PlayerSetupData
            {
                PlayerIndex = PLAYER_1_ID,
                DeviceName = picks[PLAYER_1_INDEX].inputDevice,
                IsNPC = picks[PLAYER_1_INDEX].inputDevice == null,
                SelectedCharacter = picks[PLAYER_1_INDEX].characterData
            };
            GameDataBridge.I.SetupPlayer(PLAYER_1_ID, player1Data);

            var player2Data = new GameDataBridge.PlayerSetupData
            {
                PlayerIndex = PLAYER_2_ID,
                DeviceName = picks[PLAYER_2_INDEX].inputDevice,
                IsNPC = picks[PLAYER_2_INDEX].inputDevice == null,
                SelectedCharacter = picks[PLAYER_2_INDEX].characterData
            };
            GameDataBridge.I.SetupPlayer(PLAYER_2_ID, player2Data);

            SelectUIManager.I.EventBus.Publish(new GameStartEvent
            {
                Player1Character = picks[PLAYER_1_INDEX].characterData,
                Player2Character = picks[PLAYER_2_INDEX].characterData,
                Player1Device = picks[PLAYER_1_INDEX].inputDevice,
                Player2Device = picks[PLAYER_2_INDEX].inputDevice,
                IsPlayer2Npc = picks[PLAYER_2_INDEX].inputDevice == null
            });

            SceneLoader.I.LoadBattleSceneAsync().Forget();
        }
    }
}