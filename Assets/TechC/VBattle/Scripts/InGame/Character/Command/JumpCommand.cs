namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// ジャンプコマンド
    /// </summary>
    public readonly struct JumpCommand : ICommand
    {
        public CommandType Type => CommandType.Jump;
    }
}
