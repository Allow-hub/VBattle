namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 攻撃種　弱・強など
    /// </summary>
    public enum AttackType
    {
        None,
        Weak,
        Strong,
        Air,
    }

    /// <summary>
    /// 攻撃方向（派生用）
    /// </summary>
    public enum AttackDirection
    {
        Neutral,    // 通常攻撃
        Forward,    // 前攻撃
        Back,       // 後ろ攻撃
        Upper,      // 上攻撃
        Downer,     // 下攻撃
    }

    /// <summary>
    /// 攻撃コマンド
    /// 攻撃種と攻撃方向の保持し、渡す
    /// </summary>
    public struct AttackCommand : ICommand
    {
        public CommandType Type => CommandType.Attack;
        public AttackType AttackType { get; }
        public AttackDirection AttackDirection { get; }

        public AttackCommand(AttackType attackType, AttackDirection attackDirection)
        {
            AttackType = attackType;
            AttackDirection = attackDirection;
        }
    }
}