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

    /// <summary>
    /// コマンドインターフェース
    /// 継承先はデータしか持たないので構造体にする
    /// </summary>
    public interface ICommand
    {
        CommandType Type { get; }
    }
}