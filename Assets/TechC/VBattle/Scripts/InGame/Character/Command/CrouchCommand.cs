namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// しゃがみコマンド
    /// </summary>
    public struct CrouchCommand : ICommand
    {
        public CommandType Type => CommandType.Crouch;

        /// <summary>
        /// true: しゃがみ開始, false: しゃがみ解除
        /// </summary>
        public bool IsPress { get; }

        public CrouchCommand(bool isPress = true)
        {
            IsPress = isPress;
        }
    }
}