using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Util;

namespace TechC.VBattle.Select.Core
{
    public class CharacterSelectManager : Singleton<CharacterSelectManager>
    {
        private const float InitializeDelay = 0.5f;

        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();

            _ = DelayUtility.StartDelayedActionAsync(InitializeDelay, () =>lizeSelectSystem);
        }

        /// <summary>
        /// キャラクター選択システムの初期化
        /// </summary>
        private void InitializeSelectSystem()
        {
            // GameDataBridgeのプレイヤー情報をクリア
            if (GameDataBridge.I == null)
            {
                CustomLogger.Error("GameDataBridgeが初期化されていません");
                return;
            }

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
            if (!SelectUIManager.I.HasPicked[0] || !SelectUIManager.I.HasPicked[1])
            {
                return;
            }

            // GameDataBridge にプレイヤー情報を設定
            var picks = SelectUIManager.I.CurrentPicks;

            // Player 1 の設定
            var player1Data = new GameDataBridge.PlayerSetupData
            {
                PlayerIndex = 0,
                DeviceName = picks[0].inputDevice,
                IsNPC = picks[0].inputDevice == null,
                SelectedCharacter = null // TODO: CharacterDataの適切な取得方法を実装
            };
            GameDataBridge.I.SetupPlayer(0, player1Data);

            // Player 2 の設定
            var player2Data = new GameDataBridge.PlayerSetupData
            {
                PlayerIndex = 1,
                DeviceName = picks[1].inputDevice,
                IsNPC = picks[1].inputDevice == null,
                SelectedCharacter = null // TODO: CharacterDataの適切な取得方法を実装
            };
            GameDataBridge.I.SetupPlayer(1, player2Data);

            CustomLogger.Info($"GameDataBridge にプレイヤー情報を設定完了: P1_NPC={player1Data.IsNPC}, P2_NPC={player2Data.IsNPC}");

            // シーン遷移
            SceneLoader.I.LoadBattleSceneAsync().Forget();
        }
    }
}