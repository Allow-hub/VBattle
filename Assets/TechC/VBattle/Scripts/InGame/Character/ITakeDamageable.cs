using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 攻撃を受けることを契約させるインターフェース
    /// </summary>
    public interface ITakeDamageable
    {
        void TakeDamage(float damage, Vector3 knockbackDirection, float knockbackForce, float stunDuration = 0.3f);
    }
}