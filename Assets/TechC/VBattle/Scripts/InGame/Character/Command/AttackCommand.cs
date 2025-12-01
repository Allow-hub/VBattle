namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 攻撃種　弱・強など
    /// </summary>
    public enum AttackType
    {
        Weak = 0,
        Strong = 1,
        Air,
        None,
    }

    /// <summary>
    /// 攻撃方向（派生用）
    /// </summary>
    public enum AttackDirection
    {
        Neutral,    // 通常攻撃
        Left,       // 左攻撃
        Right,      // 右攻撃
        Up,         // 上攻撃
        Down,       // 下攻撃
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