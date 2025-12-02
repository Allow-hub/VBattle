using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Util;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC.VBattle.Select.Core
{
    public class CharacterSelectManager : Singleton<CharacterSelectManager>
    {
        private const float INITIALIZE_DELAY = 0.5f;

        protected override bool UseDontDestroyOnLoad => false;


        public override void Init()
        {
            base.Init();

            _ = DelayUtility.StartDelayedActionAsync(INITIALIZE_DELAY, () =>
            {
                // 初期化処理 - GameDataBridge のプレイヤー情報をクリア
                if (GameDataBridge.I != null)
                {
                    GameDataBridge.I.SetupPlayer(0, null); // Player1 情報をクリア
                    GameDataBridge.I.SetupPlayer(1, null); // Player2 情報をクリア
                    CustomLogger.Info("GameDataBridge のプレイヤー情報をクリアしました");
                }

                SelectUIManager.I.OnStartGamePicked += DicidePick;
            });
        }

        private void DicidePick()
        {
            if (!SelectUIManager.I.HasPicked[0] || !SelectUIManager.I.HasPicked[1])
            {
                CustomLogger.Warning("まだ全プレイヤーがピックしていません");
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
                SelectedCharacter = picks[0].characterObject?.GetComponent<TechC.VBattle.InGame.Character.CharacterData>()
            };
            GameDataBridge.I.SetupPlayer(0, player1Data);

            // Player 2 の設定
            var player2Data = new GameDataBridge.PlayerSetupData
            {
                PlayerIndex = 1,
                DeviceName = picks[1].inputDevice,
                IsNPC = picks[1].inputDevice == null,
                SelectedCharacter = picks[1].characterObject?.GetComponent<TechC.VBattle.InGame.Character.CharacterData>()
            };
            GameDataBridge.I.SetupPlayer(1, player2Data);

            CustomLogger.Info($"GameDataBridge にプレイヤー情報を設定完了: P1_NPC={player1Data.IsNPC}, P2_NPC={player2Data.IsNPC}");

            // シーン遷移
            SceneLoader.I.LoadBattleSceneAsync().Forget();
        }
    }
}