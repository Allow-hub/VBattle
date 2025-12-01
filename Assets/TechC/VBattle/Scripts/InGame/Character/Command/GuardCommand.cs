namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// ガードコマンド
    /// </summary>
    public struct GuardCommand : ICommand
    {
        public CommandType Type => CommandType.Guard;

        /// <summary>
        /// true: ガード開始, false: ガード解除
        /// </summary>
        public bool IsPress { get; }

        public GuardCommand(bool isPress = true)
        {
            IsPress = isPress;
        }
    }
}
