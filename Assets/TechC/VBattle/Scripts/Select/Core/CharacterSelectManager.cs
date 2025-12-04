using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Managers;

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
            GameDataBridge.I.SetupPlayer(0, null);
            GameDataBridge.I.SetupPlayer(1, null);

            // イベント購読
            SelectUIManager.I.OnStartGamePicked += OnGameStartRequested;
        }

        /// <summary>
        /// ゲーム開始リクエスト処理
        /// </summary>
        private void OnGameStartRequested()
        {
            if (!SelectUIManager.I.HasPicked[0] || !SelectUIManager.I.HasPicked[1]) return;

            // GameDataBridge にプレイヤー情報を設定
            var picks = SelectUIManager.I.CurrentPicks;

            // Player 1 の設定
            var player1Data = new GameDataBridge.PlayerSetupData
            {
                PlayerIndex = 1,
                DeviceName = picks[0].inputDevice,
                IsNPC = picks[0].inputDevice == null,
                SelectedCharacter = null // TODO: CharacterDataの適切な取得方法を実装
            };
            GameDataBridge.I.SetupPlayer(0, player1Data);

            // Player 2 の設定
            var player2Data = new GameDataBridge.PlayerSetupData
            {
                PlayerIndex = 2,
                DeviceName = picks[1].inputDevice,
                IsNPC = picks[1].inputDevice == null,
                SelectedCharacter = null // TODO: CharacterDataの適切な取得方法を実装
            };
            GameDataBridge.I.SetupPlayer(1, player2Data);

            // シーン遷移
            SceneLoader.I.LoadBattleSceneAsync().Forget();
        }
    }
}