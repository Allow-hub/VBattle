using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// ニュートラルのステート
    /// </summary>
    public class NeutralState : CharacterState
    {
        private float elapsedTime = 0;
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
            var data = controller.Data;
            
            while (!ct.IsCancellationRequested)
            {
                RecoverGuardPower(data);
                
                await UniTask.Yield(ct);
            }
            return this;
        }

        private void RecoverGuardPower(CharacterData data)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime < data.GuardRecoveryInterval) return;
            // ガードパワーの段階的な回復
            if (controller.CurrentGuardPower >= data.GuardPower) return;
            var deltaTime = Time.deltaTime;
            var recoveryAmount = data.GuardRecoverySpeed * deltaTime;

            // 最大値を超えないように制限
            var newPower = Mathf.Min(
                controller.CurrentGuardPower + recoveryAmount,
                data.GuardPower
            );

            controller.SetGuardPower(newPower);
        }

        public override void OnExit()
        {
        }
    }
}
