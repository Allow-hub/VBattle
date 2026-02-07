using UnityEngine;
using TechC.VBattle.InGame.Input;

namespace TechC.VBattle.InGame.Npc
{
    /// <summary>
    /// NPC用入力マネージャー
    /// BaseInputManagerを継承し、AIからの入力指示を受け付ける
    /// </summary>
    public class AIInputManager : BaseInputManager
    {
        /// <summary>
        /// 移動入力を設定
        /// </summary>
        /// <param name="direction">移動方向（-1〜1）</param>
        public void SetMoveInput(Vector2 direction)
        {
            SetMove(direction);

            if (Mathf.Abs(direction.x) > 0.1f)
            {
                OnButtonDown(InputButton.Move);
            }
            else
            {
                OnButtonUp(InputButton.Move);
            }
        }

        /// <summary>
        /// ジャンプ入力
        /// </summary>
        /// <param name="press">押す/離す</param>
        public void SetJumpInput(bool press)
        {
            if (press)
            {
                SetMove(new Vector2(holdX, 1f));
                OnButtonDown(InputButton.Jump);
            }
            else
            {
                SetMove(new Vector2(holdX, 0f));
                OnButtonUp(InputButton.Jump);
            }
        }

        /// <summary>
        /// しゃがみ入力
        /// </summary>
        /// <param name="press">押す/離す</param>
        public void SetCrouchInput(bool press)
        {
            if (press)
            {
                SetMove(new Vector2(holdX, -1f));
                OnButtonDown(InputButton.Crouch);
            }
            else
            {
                SetMove(new Vector2(holdX, 0f));
                OnButtonUp(InputButton.Crouch);
            }
        }

        /// <summary>
        /// ガード入力
        /// </summary>
        /// <param name="press">押す/離す</param>
        public void SetGuardInput(bool press)
        {
            if (press)
            {
                OnButtonDown(InputButton.Guard);
            }
            else
            {
                OnButtonUp(InputButton.Guard);
            }
        }

        /// <summary>
        /// 弱攻撃入力
        /// </summary>
        /// <param name="direction">攻撃方向（オプション）</param>
        public void SetWeakAttackInput(Vector2 direction = default)
        {
            if (direction != default && direction.magnitude > 0.1f)
            {
                SetMove(direction);
            }
            OnButtonDown(InputButton.WeakAttack);
        }

        /// <summary>
        /// 弱攻撃解除
        /// </summary>
        public void ReleaseWeakAttack()
        {
            OnButtonUp(InputButton.WeakAttack);
            SetMove(Vector2.zero);
        }

        /// <summary>
        /// 強攻撃入力
        /// </summary>
        /// <param name="direction">攻撃方向（オプション）</param>
        public void SetStrongAttackInput(Vector2 direction = default)
        {
            if (direction != default && direction.magnitude > 0.1f)
            {
                SetMove(direction);
            }
            OnButtonDown(InputButton.StrongAttack);
        }

        /// <summary>
        /// 強攻撃解除
        /// </summary>
        public void ReleaseStrongAttack()
        {
            OnButtonUp(InputButton.StrongAttack);
            SetMove(Vector2.zero);
        }

        /// <summary>
        /// すべての入力をリセット
        /// </summary>
        public void ResetAllInputs()
        {
            SetMove(Vector2.zero);
            holdButtons = InputButton.None;
        }
    }
}
