namespace TechC.VBattle.InGame
{
    // コマンドインターフェース
    public interface ICommand
    {
        /// <summary>
        /// 実処理
        /// </summary>
        void Execute();

        /// <summary>
        /// 巻き戻し処理
        /// </summary>
        void Undo();

        /// <summary>
        /// 終了したかどうか
        /// </summary>
        bool IsFinished { get; }
        /// <summary>
        /// コマンド割り込み時に強制的に終了させるメソッド
        /// </summary>
        void ForceFinish();
    }
}