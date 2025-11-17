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
        public void Move(Vector2 direction)
        {
            Vector3 move = new Vector3(direction.x, 0, 0) * characterData.MoveSpeed * Time.deltaTime;
            rb.MovePosition(transform.position + move);
        }

        public void Jump()
        {
            if (IsGrounded())
            {
                rb.AddForce(Vector3.up * characterData.JumpPower, ForceMode.Impulse);
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
        }

        public void StartGuard()
        {
            guardObj.SetActive(true);
            isGuarding = true;
            stateMachine.ChangeState(GetState<GuardState>());
        }

        public void EndGuard()
        {
            guardObj.SetActive(false);
            isGuarding = false;
            stateMachine.ChangeState(GetState<NeutralState>());
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(float damage, float stunDuration = 0.3f)
        {
            var damageState = GetState<DamageState>();
            damageState.SetStunDuration(stunDuration);
            stateMachine.ChangeState(damageState);
        }
    }
}