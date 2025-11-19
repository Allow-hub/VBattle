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
        private float smoothSpeed = 0;
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
                UpdateMovementAnimation();
                ApplyGroundFriction();
                RecoverGuardPower(data);

                await UniTask.Yield(ct);
            }
            return this;
        }

        /// <summary>
        /// MoveAnimationのBlendTreeを更新
        /// </summary>
        private void UpdateMovementAnimation()
        {

            var velocity = controller.Rb.velocity;

            var horizontalSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
            var maxSpeed = controller.Data.DashMoveSpeed;

            var targetSpeed = Mathf.Clamp01(horizontalSpeed / maxSpeed);

            smoothSpeed = Mathf.Lerp(smoothSpeed, targetSpeed, 0.15f);

            controller.Anim.SetFloat(AnimatorParam.XSpeed, smoothSpeed);
        }

        /// <summary>
        /// 移動入力がないとき横移動を減速
        /// </summary>
        private void ApplyGroundFriction()
        {
            // 地上でないなら摩擦なし
            if (!controller.IsGrounded()) return;

            // 移動入力があるなら摩擦なし
            if (controller.CommandInvoker.HasMoveInput) return;

            var rb = controller.GetComponent<Rigidbody>();
            var vel = rb.velocity;

            // XZ のみ減速
            Vector3 horizontal = new Vector3(vel.x, 0, 0);

            // 徐々に減速させる
            horizontal = Vector3.Lerp(horizontal, Vector3.zero, 10f * Time.deltaTime);

            rb.velocity = new Vector3(horizontal.x, vel.y, horizontal.z);
        }

        /// <summary>
        /// ガード力の回復、Enterから数秒経って回復
        /// </summary>
        /// <param name="data"></param>
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
            elapsedTime = 0;
        }
    }
}
