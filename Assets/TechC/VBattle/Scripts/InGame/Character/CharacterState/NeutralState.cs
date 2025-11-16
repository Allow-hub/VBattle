using System.Threading;
using Cysharp.Threading.Tasks;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// ニュートラルのステート
    /// </summary>
    public class NeutralState : CharacterState
    {
        public NeutralState(CharacterController controller) : base(controller) { }

        public override bool CanExecuteCommand<T>(T command)
        {
            // 地上ではすべての行動が可能
            return command.Type == CommandType.Move ||
                   command.Type == CommandType.Jump ||
                   command.Type == CommandType.Attack ||
                   command.Type == CommandType.Guard;
        }

        public override void OnEnter(CharacterState prevState)
        {
        }

        public override async UniTask<CharacterState> OnUpdate(CancellationToken ct)
        {
            // コマンドによって状態が変わるまで待機
            while (!ct.IsCancellationRequested)
            {
                await UniTask.Yield(ct);
            }
            return this;
        }

        public override void OnExit()
        {
        }
    }
}
