namespace TechC.VBattle.InGame
{
    /// <summary>
    /// コマンドの種類
    /// </summary>
    public enum CommandType
    {
        Move,
        Attack,
        Jump,
        Guard
    }

    // コマンドインターフェース
    public interface ICommand
    {
        CommandType Type { get; }
        public void Execute();
    }
}