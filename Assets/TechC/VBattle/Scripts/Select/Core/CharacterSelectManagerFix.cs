using System.Collections;
using System.Collections.Generic;
using TechC.VBattle.Core.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC.VBattle.Select.Core
{
    public class CharacterSelectManagerFix : Singleton<CharacterSelectManagerFix>
    {
        // private const float initializeDelay = 0.5f;
        // protected override bool UseDontDestroyOnLoad => false;


        // protected override void Init()
        // {
        //     base.Init();

        //     DelayUtility.StartDelayedAction(this, initializeDelay, () =>
        //     {
        //         // 初期化処理
        //         // プレイヤー情報を一旦コピー
        //         var playerInfos = new List<(GameObject prefab, int playerId, InputDevice inputDevice)>(GameManager.I.GetPlayerInfo());

        //         foreach (var info in playerInfos)
        //         {
        //             GameManager.I.RemovePlayerById(info.playerId);
        //         }

        //         if (SelectUIManager.I == null)
        //         {
        //             Debug.Log("SelectUIManagerの初期化が済んでいません");
        //             return;
        //         }
        //         SelectUIManager.I.OnStartGamePicked += DicidePick;
        //     });
        // }
        // private void DicidePick()
        // {
        //     if (!SelectUIManager.I.HasPicked[0] || !SelectUIManager.I.HasPicked[1])
        //     {
        //         Debug.Log("まだ全プレイヤーがピックしていません");
        //         return;
        //     }
        //     bool isNpc = false;
        //     foreach (var pick in SelectUIManager.I.CurrentPicks)
        //     {
        //         GameManager.I.RegisterPlayer(pick.characterObject, pick.playerId, pick.inputDevice);
        //         if (pick.inputDevice == null)
        //             isNpc = true;
        //     }
        //     GameManager.I.SetIsNpc(isNpc);//NPCかどうかを設定
        //     GameManager.I.ChangeBattleState();
        // }
    }
}