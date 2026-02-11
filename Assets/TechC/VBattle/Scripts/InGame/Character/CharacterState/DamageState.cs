using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Util;
using TechC.VBattle.Systems;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    public class DamageState : CharacterState
    {
        private float damageStunDuration = 0.3f;
        private AttackData attackData;
        private Vector3 attackerPosition;
        private Vector3 attackerForward; // ★ 攻撃者の向きを保存

        private float knockbackForce;
        private Vector3 knockbackDirection;
        private bool hasKnockback = false;

        private bool isWallBouncing = false;
        private float wallBounceElapsedTime = 0f;
        private int wallHitDirection = 0;
        private bool hasWallBounced = false;

        public override int Priority => 10;

        public DamageState(CharacterController controller) : base(controller) { }

        public override bool CanExecuteCommand<T>(T command) => false;

        public override void OnEnter(CharacterState prevState)
        {
            // ★ OnEnterで初期化
            controller.Anim.SetBool(AnimatorParam.IsHitting, false);
            controller.Anim.SetBool(AnimatorParam.IsWallHitting, false);
            
            isWallBouncing = false;
            wallBounceElapsedTime = 0f;
            hasWallBounced = false;

            AnimatorUtil.SetAnimatorBoolExclusive(controller.Anim, AnimatorParam.IsHitting);

            ApplyKnockback();
        }

        public override async UniTask<CharacterState> OnUpdate(CancellationToken ct)
        {
            float elapsedTime = 0f;

            while (elapsedTime < damageStunDuration)
            {
                if (!hasWallBounced && attackData?.causesWallBounce == true)
                {
                    if (CheckWallBehind(out int hitDir))
                    {
                        wallHitDirection = hitDir;
                        StartWallBounce();
                        elapsedTime = 0f;
                        continue;
                    }
                }

                if (isWallBouncing)
                    wallBounceElapsedTime += Time.deltaTime;

                await UniTask.Yield(ct);
                elapsedTime += Time.deltaTime;
            }

            if (!controller.IsGrounded())
                return controller.GetState<AirState>();

            return controller.GetState<NeutralState>();
        }

        public override void OnExit()
        {
            // ★ OnExitでは最低限のクリーンアップのみ
            controller.Anim.SetBool(AnimatorParam.IsHitting, false);
            controller.Anim.SetBool(AnimatorParam.IsWallHitting, false);
        }

        public void SetDamageInfo(AttackData data, Vector3 attackerPos, Vector3 attackerFwd)
        {
            attackData = data;
            attackerPosition = attackerPos;
            attackerForward = attackerFwd;

            if (data == null) return;

            damageStunDuration = data.hitStunDuration;

            if (data.knockbackForce > 0f)
            {
                knockbackForce = data.knockbackForce;
                knockbackDirection = data.knockbackDirection;
                hasKnockback = true;
            }
        }

        public void SetStunDuration(float duration)
        {
            damageStunDuration = duration;
        }

        public void SetKnockback(Vector3 attackerPos, Vector3 attackerFwd, float force, Vector3 dir)
        {
            attackerPosition = attackerPos;
            attackerForward = attackerFwd;
            knockbackForce = force;
            knockbackDirection = dir;
            hasKnockback = knockbackForce > 0.01f;
        }

        /// <summary>
        /// ノックバック処理
        /// 被弾者の向きと攻撃者の位置を考慮した2D格闘ゲーム用
        /// </summary>
        private void ApplyKnockback()
        {
            if (!hasKnockback || knockbackForce <= 0f)
                return;

            var rb = controller.Rb;
            if (rb == null)
                return;

            // ★ 被弾者の向きを取得
            Vector3 victimForward = controller.transform.forward;
            victimForward.y = 0f;
            victimForward.Normalize();

            // ★ 攻撃者→被弾者の方向を計算
            Vector3 attackToVictim = controller.transform.position - attackerPosition;
            attackToVictim.y = 0f;

            Vector3 knockbackDir;

            // ★ 距離が極端に近い場合
            if (attackToVictim.sqrMagnitude < 0.01f)
            {
                // 被弾者の後方に飛ばす
                knockbackDir = -victimForward;
                CustomLogger.Warning("Knockback: Very close range, using victim backward", LogTagUtil.TagState);
            }
            else
            {
                attackToVictim.Normalize();

                // ★ 攻撃者が被弾者のどちら側にいるかを判定
                // 内積 > 0 なら前方、< 0 なら後方
                float dotProduct = Vector3.Dot(victimForward, attackToVictim);

                if (dotProduct > 0.1f)
                {
                    // 攻撃者は前方 → 被弾者の後方に飛ばす
                    knockbackDir = -victimForward;
                }
                else if (dotProduct < -0.1f)
                {
                    // 攻撃者は後方 → 被弾者の前方に飛ばす
                    knockbackDir = victimForward;
                }
                else
                {
                    // 真横から攻撃 → 攻撃方向の逆に飛ばす
                    knockbackDir = attackToVictim;
                }

                CustomLogger.Info(
                    $"Knockback: dotProduct={dotProduct}, victimForward={victimForward}, attackToVictim={attackToVictim}",
                    LogTagUtil.TagState);
            }

            // ★ knockbackDirection.x が負なら方向を反転（引き寄せ技用）
            if (knockbackDirection.x < 0)
            {
                knockbackDir = -knockbackDir;
            }

            // ★ 水平方向と垂直方向を合成
            float horizontalForce = Mathf.Abs(knockbackDirection.x) * knockbackForce;
            float verticalForce = knockbackDirection.y * knockbackForce;

            Vector3 finalVelocity = knockbackDir * horizontalForce + Vector3.up * verticalForce;

            // ★ VelocityChangeで適用
            rb.velocity = Vector3.zero;
            rb.AddForce(finalVelocity, ForceMode.VelocityChange);

            CustomLogger.Info(
                $"Knockback Applied: dir={knockbackDir}, finalVel={finalVelocity}, victimForward={victimForward}",
                LogTagUtil.TagState);
        }

        private bool CheckWallBehind(out int hitDirection)
        {
            hitDirection = 0;

            var wallCheckDistance = 0.6f;
            Vector3 origin = controller.transform.position + Vector3.up * 0.5f;

            Vector3 backward = -controller.transform.forward;
            bool hitBack = Physics.Raycast(origin, backward, wallCheckDistance, LayerMask.GetMask("Wall"));

            Vector3 forward = controller.transform.forward;
            bool hitForward = Physics.Raycast(origin, forward, wallCheckDistance, LayerMask.GetMask("Wall"));

            Debug.DrawRay(origin, backward * wallCheckDistance, hitBack ? Color.red : Color.yellow, 2f);
            Debug.DrawRay(origin, forward * wallCheckDistance, hitForward ? Color.green : Color.cyan, 2f);

            if (hitForward)
            {
                hitDirection = 1;
                return true;
            }

            if (hitBack)
            {
                hitDirection = -1;
                return true;
            }

            return false;
        }

        private void StartWallBounce()
        {
            if (attackData == null || !attackData.causesWallBounce)
                return;

            isWallBouncing = true;
            wallBounceElapsedTime = 0f;

            controller.Anim.SetBool(AnimatorParam.IsHitting, false);
            AnimatorUtil.SetAnimatorBoolExclusive(controller.Anim, AnimatorParam.IsWallHitting);

            damageStunDuration = attackData.wallBounceTime;

            var rb = controller.Rb;
            if (rb == null)
                return;

            AudioManager.I?.PlaySE(Audio.SEID.WallHit);
            EffectFactory.I?.GetEffectObj(
                EffectFactory.I?.DebrisEffectPrefab,
                controller.transform.position,
                Quaternion.identity);

            Vector3 bounceDirection = -wallHitDirection * controller.transform.forward;
            bounceDirection.y = 0;
            bounceDirection.Normalize();

            Vector3 bounceVelocity =
                bounceDirection * attackData.wallBounceForce +
                Vector3.up * attackData.wallBounceVerticalBoost;

            rb.velocity = bounceVelocity;

            CustomLogger.Info(
                $"WallBounce: dir={bounceDirection}, vel={bounceVelocity}",
                LogTagUtil.TagState);

            hasWallBounced = true;
        }
    }
}