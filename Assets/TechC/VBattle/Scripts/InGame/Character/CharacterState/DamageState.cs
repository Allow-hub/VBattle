using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Util;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// ダメージ状態
    /// </summary>
    public class DamageState : CharacterState
    {
        private float damageStunDuration = 0.3f;
        private AttackData attackData;
        private Vector3 attackerPosition; // 変更: 攻撃者位置を保持
        private float knockbackForce;
        private Vector3 knockbackDirection;
        private bool hasKnockback = false;

        public DamageState(CharacterController controller) : base(controller) { }

        public override bool CanExecuteCommand<T>(T command)
        {
            return false;
        }

        public override void OnEnter(CharacterState prevState)
        {
            controller.Anim.SetBool(AnimatorParam.IsHitting, false);
            AnimatorUtil.SetAnimatorBoolExclusive(controller.Anim, AnimatorParam.IsHitting);
            ApplyKnockback();
        }

        public override async UniTask<CharacterState> OnUpdate(CancellationToken ct)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(damageStunDuration), cancellationToken: ct);

            if (!controller.IsGrounded())
                return controller.GetState<AirState>();

            return controller.GetState<NeutralState>();
        }

        public override void OnExit()
        {
            controller.Anim.SetBool(AnimatorParam.IsHitting, false);
            controller.Anim.SetBool(AnimatorParam.IsWallHitting, false);
            hasKnockback = false;
            knockbackForce = 0f;
            attackerPosition = Vector3.zero;
        }

        /// <summary>
        /// AttackDataと攻撃者位置から情報を設定
        /// </summary>
        public void SetDamageInfo(AttackData data, Vector3 attackerPos)
        {
            attackData = data;
            attackerPosition = attackerPos;

            if (data == null) return;
            damageStunDuration = data.hitStunDuration;

            if (data.knockbackForce > 0f)
            {
                knockbackForce = data.knockbackForce;
                knockbackDirection = data.knockbackDirection;
                hasKnockback = true;
            }
        }

        /// <summary>
        /// スタン時間のみ設定
        /// </summary>
        public void SetStunDuration(float duration)
        {
            damageStunDuration = duration;
        }

        /// <summary>
        /// ノックバック情報を設定（攻撃者位置ベース）
        /// </summary>
        public void SetKnockback(Vector3 attackerPos, float force, Vector3 dir)
        {
            attackerPosition = attackerPos;
            knockbackForce = force;
            knockbackDirection = dir;
            hasKnockback = knockbackForce > 0.01f;
        }

        /// <summary>
        /// 攻撃者位置とknockbackDirectionからノックバック適用
        /// </summary>
        private void ApplyKnockback()
        {
            if (!hasKnockback || knockbackForce <= 0f) return;

            var rb = controller.Rb;
            if (rb == null) return;

            // 攻撃者から被ダメージ者への方向ベクトル
            Vector3 fromAttacker = (controller.transform.position - attackerPosition).normalized;
            
            // knockbackDirectionを攻撃者基準の方向に変換
            Vector3 finalDirection = fromAttacker.x * knockbackDirection.x * Vector3.right +
                                    knockbackDirection.y * Vector3.up +
                                    fromAttacker.z * knockbackDirection.z * Vector3.forward;
            
            finalDirection = finalDirection.normalized;

            // ノックバック適用
            rb.velocity = finalDirection * knockbackForce;

            CustomLogger.Info($"Knockback: attackerPos={attackerPosition}, dir={finalDirection}, force={knockbackForce}", LogTagUtil.TagState);
        }
    }
}