using System.Threading;
using Cysharp.Threading.Tasks;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// キャラクターの状態（State）の基底クラス
    /// </summary>
    public abstract class CharacterState
    {
        /// <summary>
        /// このStateが所属するCharacterController
        /// </summary>
        protected CharacterController controller;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="controller">このStateに紐づくCharacterController</param>
        public CharacterState(CharacterController controller)
        {
            this.controller = controller;
        }

        /// <summary>
        /// このStateで受け付けるCommandかどうかをチェック
        /// </summary>
        /// <typeparam name="T">コマンドの型</typeparam>
        /// <param name="command">チェック対象のコマンド</param>
        /// <returns>受け付け可能であればtrue、それ以外はfalse</returns>
        public abstract bool CanExecuteCommand<T>(T command) where T : struct, ICommand;

        /// <summary>
        /// コマンドが実行された際の追加処理
        /// </summary>
        /// <typeparam name="T">コマンドの型</typeparam>
        /// <param name="command">実行されたコマンド</param>
        public virtual void OnCommandExecuted<T>(T command) where T : struct, ICommand { }

        /// <summary>
        /// 状態が開始された際に呼ばれる処理
        /// </summary>
        /// <param name="prevState">遷移前の状態</param>
        public virtual void OnEnter(CharacterState prevState) { }

        /// <summary>
        /// 状態の更新処理
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        /// <returns>次に遷移する状態（遷移なしの場合は自身）</returns>
        public abstract UniTask<CharacterState> OnUpdate(CancellationToken ct);

        /// <summary>
        /// 状態が終了する際に呼ばれる処理
        /// </summary>
        public virtual void OnExit() { }
    }
}