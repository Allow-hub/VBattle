using TechC.VBattle.Core.Util;
using TechC.VBattle.InGame.Events;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// キャラクターのCommandから呼ばれるアクション群
    /// </summary>
    public partial class CharacterController
    {
        /// <summary>
        /// 左右の移動
        /// </summary>
        /// <param name="direction">向き</param>
        public void Move(Vector2 direction, bool isDashing)
        {
            if (direction.sqrMagnitude > 0.01f)
            {
                bool isGrounded = IsGrounded();
                float moveSpeed = isDashing ? characterData.DashMoveSpeed : characterData.MoveSpeed;
                // 空中では制御係数を適用
                if (!isGrounded)
                {
                    moveSpeed *= characterData.AirControlMultiplier;
                }

                // 移動方向を計算
                Vector3 moveDirection = new Vector3(direction.x, 0, 0).normalized;
                Vector3 targetVelocity = moveDirection * moveSpeed;

                // 現在の速度を取得してY軸は保持
                Vector3 currentVelocity = rb.velocity;
                Vector3 velocityChange = new Vector3(
                    targetVelocity.x - currentVelocity.x,
                    0,
                    0
                );

                // 加速度を調整（地上は即座、空中はマイルド）
                float acceleration = isGrounded ? 0.8f : 0.3f;
                rb.AddForce(velocityChange * acceleration, ForceMode.VelocityChange);

                // 最高速度の制限
                Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                if (horizontalVelocity.magnitude > moveSpeed)
                {
                    horizontalVelocity = horizontalVelocity.normalized * moveSpeed;
                    rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
                }

                // キャラクターの向きを移動方向に変更
                if (moveDirection.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), 0.9f);
            }
        }

        public void Jump()
        {
            if (currentJumpCount < maxJumpCount)
            {
                currentJumpCount++;

                AnimatorUtil.SetAnimatorBoolExclusive(anim, AnimatorParam.IsJumping);

                // Y速度をリセットして二段目の高さが安定するようにする
                var v = rb.velocity;
                v.y = 0;
                rb.velocity = v;
                if (currentJumpCount == 1)
                    rb.AddForce(Vector3.up * characterData.JumpPower, ForceMode.Impulse);
                else
                    rb.AddForce(Vector3.up * characterData.DoubleJumpPower, ForceMode.Impulse);

                // 空中状態へ遷移
                stateMachine.ChangeState(GetState<AirState>());
            }
        }


        /// <summary>
        /// 攻撃、SOから構築
        /// </summary>
        /// <param name="type">攻撃種</param>
        /// <param name="direction">入力方向</param>
        public void Attack(AttackType type, AttackDirection direction)
        {
            CurrentAttackType = type;
            CurrentAttackDirection = direction;
            if (stateMachine.CurrentState == GetState<AttackState>()) return;
            stateMachine.ChangeState(GetState<AttackState>());
        }

        public void StartGuard()
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);//x速度を0に
            AnimatorUtil.SetAnimatorBoolExclusive(anim, AnimatorParam.IsGuarding);//ガード以外のアニメーションを停止
            guardObj.SetActive(true);
            isGuarding = true;
            stateMachine.ChangeState(GetState<GuardState>());
        }

        public void EndGuard()
        {
            anim.SetBool(AnimatorParam.IsGuarding, false);
            guardObj.SetActive(false);
            isGuarding = false;
            stateMachine.ChangeState(GetState<NeutralState>());
        }

        public void StartCrouch()
        {
            stateMachine.ChangeState(GetState<CrouchState>());
        }

        public void EndCrouch()
        {
            stateMachine.ChangeState(GetState<NeutralState>());
        }

        /// <summary>
        /// 攻撃をEventBusから処理する、基本的にPlayer同士の攻撃はこちら
        /// </summary>
        /// <param name="e">EventBusで処理された結果</param>
        private void HandleAttackResult(AttackResultEvent e)
        {
            if (e.attacker == this) return;//攻撃者が自分の場合は除外
            if (!e.isHit) return;//攻撃を受けてない場合
            TakeDamage(e.attackData.damage, e.attackData.hitStunDuration);
        }

        /// <summary>
        /// ダメージを受ける.アイテム等はインターフェースを介してこちらでダメージを
        /// </summary>
        public void TakeDamage(float damage, float stunDuration = 0.3f)
        {
            var damageState = GetState<DamageState>();
            damageState.SetStunDuration(stunDuration);
            stateMachine.ChangeState(damageState);
        }
    }
}