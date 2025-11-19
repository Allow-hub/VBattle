using System.Numerics;
using System.Threading;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Util;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// しゃがみ状態
    /// </summary>
    public class CrouchState : CharacterState
    {
        public CrouchState(CharacterController controller) : base(controller) { }

        public override bool CanExecuteCommand<T>(T command)
        {
            // しゃがみ中に許可するのは「しゃがみ解除」だけ
            return command.Type == CommandType.Crouch;
        }

        public override void OnEnter(CharacterState prevState)
        {
            AnimatorUtil.SetAnimatorBoolExclusive(controller.Anim, AnimatorParam.IsCrouching);
        }

        public override async UniTask<CharacterState> OnUpdate(CancellationToken ct)
        {
            const float decelRate = 2.3f;  // 減速スピード（大きいほど早く止まる）

            while (!ct.IsCancellationRequested)
            {
                // 現在の速度
                var vel = controller.Rb.velocity;

                // Y軸は維持しつつ、XZのみ減衰
                var targetVel = new UnityEngine.Vector3(0, vel.y, 0);

                // 徐々に targetVel（= XZゼロ）に近づける
                vel = UnityEngine.Vector3.Lerp(
                    vel,
                    targetVel,
                    decelRate * UnityEngine.Time.deltaTime
                );

                controller.Rb.velocity = vel;

                await UniTask.Yield(ct);
            }

            return this;
        }

        public override void OnExit()
        {
            controller.Anim.SetBool(AnimatorParam.IsCrouching, false);
        }
    }
}