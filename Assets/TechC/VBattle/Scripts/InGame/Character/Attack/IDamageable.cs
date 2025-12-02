using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 攻撃を受けることができる契約
    /// </summary>
    public interface IDamageable
    {
        GameObject GameObject { get; }
        bool IsInvincible { get; }
        bool IsGuarding { get; }

        /// <summary>
        /// Eventを介するDamageの処理
        /// </summary>
        /// <param name="attackData"></param>
        void TakeDamage(AttackData attackData, Vector3 attackerPosition, int damage);

        /// <summary>
        /// 飛び道具などのCharacterと主従の関係がなくなったものが使う
        /// </summary>
        /// <param name="damage">ダメージ</param>
        /// <param name="knockbackDirection">ノックバック方向</param>
        /// <param name="knockbackForce">ノックバック力</param>
        /// <param name="stunDuration">スタン継続時間</param>
        void TakeDamage(int damage, Vector3 attackerPosition, Vector3 knockbackDirection, float knockbackForce, float stunDuration = 0.3f);
    }
}
