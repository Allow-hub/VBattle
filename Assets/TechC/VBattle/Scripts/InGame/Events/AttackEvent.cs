using TechC.VBattle.InGame.Character;
using UnityEngine;

namespace TechC.VBattle.InGame.Events
{
    /// <summary>
    /// 攻撃判定モード
    /// </summary>
    public enum HitDetectionMode
    {
        UseSelf,//自分自身のコライダーに判定がある
        OverlapSphere,//指定位置を中心とした球形範囲に判定がある
        None
    }

    /// <summary>
    /// 攻撃リクエスト
    /// </summary>
    public class AttackRequestEvent : IBattleEvent
    {
        public IAttacker attacker;                // 攻撃者
        public AttackData attackData;             // 使用された攻撃データ
        public Vector3 hitPosition;               // 攻撃判定位置
        public Collider[] hitTargets;             // 攻撃判定にヒットしたコライダー群
    }

    /// <summary>
    /// 攻撃判定結果
    /// </summary>
    public class AttackResultEvent : IBattleEvent
    {
        public IAttacker attacker;                // 攻撃者
        public Character.CharacterController target;        // 被攻撃者（現状はCharacterControllerのみ）
        public AttackData attackData;             // 使用された攻撃データ
        public bool isHit;                        // ヒットしたか
        public bool isCounter;                    // カウンターヒットか
        public bool isGuard;                      // ガードしたかどうか
        public int damage;                        // 実際のダメージ量
    }

    /// <summary>
    /// 必殺技ゲージ変化イベント
    /// </summary>
    public class SpecialGaugeChangedEvent : IBattleEvent
    {
        public int PlayerIndex;
        public float CurrentGauge;
        public float MaxGauge;
        public float Percentage; // 0-1
    }
}