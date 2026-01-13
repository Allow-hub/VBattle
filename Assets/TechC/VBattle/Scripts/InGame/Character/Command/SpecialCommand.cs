namespace TechC.VBattle.InGame
{
    /// <summary>
    /// 必殺技コマンド
    /// </summary>
    public readonly struct SpecialCommand : ICommand
    {
        public CommandType Type => CommandType.Special;
    }
}