using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Util;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 被ダメージステート
    /// </summary>
    public class DamageState : CharacterState
    {
        private float damageStunDuration = 0.3f;
        private AttackData attackData;
        private Vector3 kcockbackDir;
        private float knockbackForce;

        public DamageState(CharacterController controller) : base(controller) { }

        public override bool CanExecuteCommand<T>(T command)
        {
            // ダメージ中は一切の行動不可
            return false;
        }

        public override void OnEnter(CharacterState prevState)
        {
            controller.Anim.SetBool(AnimatorParam.IsHitting, false);
            AnimatorUtil.SetAnimatorBoolExclusive(controller.Anim, AnimatorParam.IsHitting);
        }

        public override async UniTask<CharacterState> OnUpdate(CancellationToken ct)
        {
            // ダメージ硬直時間待機
            await UniTask.Delay(TimeSpan.FromSeconds(damageStunDuration), cancellationToken: ct);

            // 空中でダメージを受けた場合
            if (!controller.IsGrounded())
                return controller.GetState<AirState>();

            // 地上なら通常状態へ
            return controller.GetState<NeutralState>();
        }

        public override void OnExit()
        {
            controller.Anim.SetBool(AnimatorParam.IsHitting, false);
            controller.Anim.SetBool(AnimatorParam.IsWallHitting, false);
        }

        /// <summary>攻撃情報を設定、Eventからの処理用</summary>
        public void SetDamageInfo(AttackData data) => attackData = data;

        /// <summary>攻撃情報を設定、Eventからの処理用</summary>
        public void SetStunDuration(float duration) => damageStunDuration = duration;

        /// <summary>攻撃方向と力</summary>
        public void SetKnockback(Vector3 knockbackDirection, float knockbackForce)
        {
            kcockbackDir = knockbackDirection;
            this.knockbackForce = knockbackForce;
        }
    }
}
