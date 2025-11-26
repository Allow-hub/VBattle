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
        public Character.CharacterController Attacker;//攻撃者
        public AttackData AttackData;//使用された攻撃データ
        public Vector3 HitPosition;//攻撃判定位置
        public Collider[] HitTargets;//攻撃判定にヒットしたコライダー群
    }

    /// <summary>
    /// 攻撃判定結果
    /// </summary>
    public struct AttackResultEvent : IBattleEvent
    {
        public Character.CharacterController Attacker;//攻撃者
        public Character.CharacterController Target;//被攻撃者
        public AttackData AttackData;//使用された攻撃データ
        public bool IsHit;
        public bool IsCounter;
        public int Damage;
    }
}