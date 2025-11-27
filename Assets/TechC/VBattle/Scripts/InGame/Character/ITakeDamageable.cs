namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 攻撃を受けることを契約させるインターフェース
    /// </summary>
    public interface ITakeDamageable
    {
        void TakeDamage(float damage, float stunDuration = 0.3f);
    }
}
