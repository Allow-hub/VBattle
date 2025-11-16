using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    // ==========================================
    // Attack状態（攻撃中）
    // ==========================================
    public class AttackState : CharacterState
    {
        private AttackType currentAttackType;
        private bool canCancel = false;
        private bool isAirAttack = false;

        public AttackState(CharacterController controller) : base(controller) { }

        public override bool CanExecuteCommand<T>(T command)
        {
            // キャンセル可能タイミングでのみ次の攻撃を受け付ける
            if (command.Type == CommandType.Attack && canCancel)
            {
            }

            return false;
        }

        public override void OnCommandExecuted<T>(T command)
        {
            if (command.Type == CommandType.Attack)
            {
            }
        }

        public override void OnEnter(CharacterState prevState)
        {
            Debug.Log("Enter Attack");
            canCancel = false;
            isAirAttack = prevState is AirState;
        }

        public override async UniTask<CharacterState> OnUpdate(CancellationToken ct)
        {
            // 発生フレーム（キャンセル不可）
            await UniTask.Delay(100, cancellationToken: ct);

            // ヒット確認フレーム（キャンセル可能）
            canCancel = true;
            await UniTask.Delay(150, cancellationToken: ct);

            // 硬直フレーム（キャンセル不可）
            canCancel = false;
            await UniTask.Delay(100, cancellationToken: ct);

            // 攻撃終了後
            if (isAirAttack)
            {
                // 空中攻撃なら空中状態に戻る
                return controller.GetState<AirState>();
            }
            else
            {
                // 地上攻撃なら地上状態に戻る
                return controller.GetState<NeutralState>();
            }
        }

        public override void OnExit()
        {
            Debug.Log("Exit Attack");
            canCancel = false;
        }
    }
}
