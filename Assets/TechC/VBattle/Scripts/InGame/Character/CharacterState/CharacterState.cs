using System.Threading;
using Cysharp.Threading.Tasks;

namespace TechC.VBattle.InGame.Character
{
    // Stateの基底クラス
    public abstract class CharacterState
    {
        protected CharacterController controller;

        public CharacterState(CharacterController controller)
        {
            this.controller = controller;
        }

        // このStateで受け付けるCommandかチェック
        public abstract bool CanExecuteCommand(ICommand command);

        // Command実行時の処理
        public virtual void OnCommandExecuted(ICommand command) { }

        // 状態の開始時
        public virtual void OnEnter(CharacterState prevState) { }

        // 状態の更新（次の状態を返す）
        public abstract UniTask<CharacterState> OnUpdate(CancellationToken ct);

        // 状態の終了時
        public virtual void OnExit() { }
    }
}
