using TechC.VBattle.InGame.Character;
using UnityEngine;

namespace TechC.VBattle.InGame.Events
{
    public enum HitDetectionMode
    {
        UseSelf,
        OverlapSphere,
        None
    }

    /// <summary>
    /// 攻撃リクエスト
    /// </summary>
    public struct AttackRequestEvent : IBattleEvent
    {
        public Character.CharacterController attacker;      // 攻撃者
        public AttackData attackData;             // 使用された攻撃データ
        public Vector3 hitPosition;               // 攻撃判定位置
        public Collider[] hitTargets;             // 攻撃判定にヒットしたコライダー群
    }

    /// <summary>
    /// 攻撃判定結果
    /// </summary>
    public struct AttackResultEvent : IBattleEvent
    {
        public Character.CharacterController attacker;      // 攻撃者
        public Character.CharacterController target;        // 被攻撃者
        public AttackData attackData;             // 使用された攻撃データ
        public bool isHit;                        // ヒットしたか
        public bool isCounter;                    // カウンターヒットか
        public bool isGuard;                      // ガードしたかどうか
        public int damage;                        // 実際のダメージ量
    }
}