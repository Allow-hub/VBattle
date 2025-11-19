using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 空中状態
    /// </summary>
    public class AirState : CharacterState
    {
        private bool hasUsedAirAction = false; // 空中行動を使ったか
        private float smoothSpeed = 0;
        public AirState(CharacterController controller) : base(controller) { }

        public override bool CanExecuteCommand<T>(T command)
        {
            // 空中では空中移動と空中攻撃のみ
            if (command.Type == CommandType.Move)
                return true;

            // 空中攻撃は1回まで
            if (command.Type == CommandType.Attack && !hasUsedAirAction)
                return true;

            return false;
        }

        public override void OnCommandExecuted<T>(T command)
        {
            if (command.Type == CommandType.Attack)
            {
                hasUsedAirAction = true;
            }
        }

        public override void OnEnter(CharacterState prevState)
        {
            hasUsedAirAction = false;
        }

        public override async UniTask<CharacterState> OnUpdate(CancellationToken ct)
        {
            // 着地判定
            while (!ct.IsCancellationRequested)
            {
                UpdateJumpAnimation();
                await UniTask.Yield(ct);

                // 地面に接地したらNeutralへ
                if (controller.IsGrounded())
                {
                    return controller.GetState<NeutralState>();
                }
            }
            return this;
        }

        public override void OnExit()
        {
            controller.Anim.SetBool(AnimatorParam.IsJumping, false);
        }

        /// <summary>
        /// ジャンプのBlendTreeを更新
        /// </summary>
        private void UpdateJumpAnimation()
        {
            var velocity = controller.Rb.velocity;

            // 上方向の速度を正規化
            // 0 = 落下最大速度, 1 = 上昇最大速度
            float maxJumpSpeed = controller.Data.JumpPower; // 事前にキャラクターのジャンプ速度を取得
            float targetSpeed = Mathf.Clamp01((velocity.y + maxJumpSpeed) / (2f * maxJumpSpeed));

            // スムージング
            smoothSpeed = Mathf.Lerp(smoothSpeed, targetSpeed, 0.15f);

            // Animatorに反映
            controller.Anim.SetFloat(AnimatorParam.YSpeed, smoothSpeed);
        }
    }
}
