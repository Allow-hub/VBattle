using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
     // ==========================================
    // Air状態（空中通常時）
    // ==========================================
    public class AirState : CharacterState
    {
        private bool hasUsedAirAction = false; // 空中行動を使ったか

        public AirState(CharacterController controller) : base(controller) { }

        public override bool CanExecuteCommand(ICommand command)
        {
            // 空中では空中移動と空中攻撃のみ
            if (command.Type == CommandType.Move)
                return true;

            // 空中攻撃は1回まで
            if (command.Type == CommandType.Attack && !hasUsedAirAction)
                return true;

            return false;
        }

        public override void OnCommandExecuted(ICommand command)
        {
            if (command.Type == CommandType.Attack)
            {
                hasUsedAirAction = true;
            }
        }

        public override void OnEnter(CharacterState prevState)
        {
            Debug.Log("Enter Air");
            hasUsedAirAction = false;
        }

        public override async UniTask<CharacterState> OnUpdate(CancellationToken ct)
        {
            // 着地判定（簡易版）
            while (!ct.IsCancellationRequested)
            {
                await UniTask.Yield(ct);
                
                // 地面に接地したらNeutralへ
                if (controller.IsGrounded)
                {
                    return controller.GetState<NeutralState>();
                }
            }
            return this;
        }

        public override void OnExit()
        {
            Debug.Log("Exit Air");
        }
    }
}
