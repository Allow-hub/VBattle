namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// ジャンプコマンド
    /// </summary>
    public struct JumpCommand : ICommand
    {
        public CommandType Type => CommandType.Jump;
    }
}
