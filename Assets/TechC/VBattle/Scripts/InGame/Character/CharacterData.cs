using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    [CreateAssetMenu(menuName = "TechC/Game/CharacterData")]
    public class CharacterData : ScriptableObject
    {
        [Tooltip("キャラクター名（内部識別用）")]
        public CharaName CharacterName;

        public GameObject CharaPrefab;
        
        [Header("移動関連")]
        [Tooltip("通常の移動速度")]
        public float MoveSpeed;
        [Tooltip("ダッシュ時の移動速度")]
        public float DashMoveSpeed;
        [Tooltip("ジャンプ力")]
        public float JumpPower;
        [Tooltip("2段ジャンプの上昇力")]
        public float DoubleJumpPower;
        [Tooltip("空中での移動制御係数（0～1）")]
        public float AirControlMultiplier = 0.7f;
        [Tooltip("急降下の速度")]
        public float FastFallSpeed = 15f;

        [Header("耐久・ガード関連")]
        [Tooltip("最大HP")]
        public int MaxHP;
        [Tooltip("ガードの耐久値")]
        public float GuardPower;
        [Tooltip("ガード中、毎フレーム減少する耐久値")]
        public float GuardDecreasePower;
        [Tooltip("ガードの回復速度（毎秒）")]
        public float GuardRecoverySpeed;
        [Tooltip("ガードの回復が始まるまでの時間（秒）")]
        public float GuardRecoveryInterval;
        [Tooltip("ガード破壊スタンの時間")]
        public float GuardBreakDuration;
    }

    /// <summary>
    /// キャラクター名（識別用）
    /// </summary>
    public enum CharaName
    {
        Ame,
        Terami
    }
}