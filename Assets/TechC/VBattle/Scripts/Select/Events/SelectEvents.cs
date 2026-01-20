using TechC.VBattle.InGame.Character;
using UnityEngine.InputSystem;

namespace TechC.VBattle.Select.Events
{
    /// <summary>
    /// デバイス割り当てイベント（IconControllerで発行）
    /// </summary>
    public struct DeviceAssignedEvent : ISelectEvent
    {
        public int PlayerId;        // 1 or 2
        public InputDevice Device;  // null = NPC
    }

    /// <summary>
    /// キャラホバーイベント（CharaButton.OnPointerEnterで発行）
    /// </summary>
    public struct CharacterHoveredEvent : ISelectEvent
    {
        public int PlayerId;           // どのプレイヤーがホバーしているか
        public CharacterData Character;
        public InputDevice Device;
        public UnityEngine.Sprite CharacterSprite;  // ホバー時に表示するスプライト
    }

    /// <summary>
    /// 選択確定イベント（CharaButton.OnPointerClickで発行）
    /// 爆散アニメーション＋立ち絵表示のトリガー
    /// </summary>
    public struct SelectionConfirmedEvent : ISelectEvent
    {
        public int PlayerId;
        public CharacterData Character;
        public InputDevice Device;
        public bool IsNpc;  // 2PがNPCかどうか
    }

    /// <summary>
    /// ゲーム開始イベント（CharacterSelectManagerで発行）
    /// DataBridge送信＋シーン遷移のトリガー
    /// </summary>
    public struct GameStartEvent : ISelectEvent
    {
        public CharacterData Player1Character;
        public CharacterData Player2Character;
        public InputDevice Player1Device;
        public InputDevice Player2Device;
        public bool IsPlayer2Npc;
    }

    /// <summary>
    /// 選択リセットイベント（キャンセルボタンで発行）
    /// </summary>
    public struct SelectionResetEvent : ISelectEvent
    {
        // データなし（リセット指示のみ）
    }
}
