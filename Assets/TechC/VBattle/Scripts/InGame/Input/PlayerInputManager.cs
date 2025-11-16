using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC.VBattle.InGame.Input
{
    /// <summary>
    /// プレイヤー入力管理
    /// </summary>
    public class PlayerInputManager : BaseInputManager
    {
        public void OnMove(InputAction.CallbackContext ctx)
        {
            var value = ctx.ReadValue<Vector2>();
            SetMove(value);

            // ジャンプをReleaseベースに
            if (value.y > 0f)
                holdButtons |= InputButton.Jump; // 押されてる状態だけ保持
            else if ((holdButtons & InputButton.Jump) != 0)
            {
                // 離した瞬間
                releasedButtons |= InputButton.Jump;
                holdButtons &= ~InputButton.Jump;
            }
        }

        /// <summary>
        /// 下入力
        /// </summary>
        /// <param name="ctx"></param>
        public void OnDown(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) { holdY = -1f; SetMove(new Vector2(holdX, holdY)); }
            if (ctx.canceled) { if (holdY < 0f) holdY = 0f; SetMove(new Vector2(holdX, holdY)); }
        }

        /// <summary>
        /// ガード
        /// </summary>
        /// <param name="ctx"></param>
        public void OnGuard(InputAction.CallbackContext ctx)
        {
            if (ctx.started) OnButtonDown(InputButton.Guard);
            if (ctx.canceled) OnButtonUp(InputButton.Guard);
        }

        /// <summary>
        /// 弱攻撃
        /// </summary>
        /// <param name="ctx"></param>
        public void OnWeakAttack(InputAction.CallbackContext ctx)
        {
            if (ctx.started) OnButtonDown(InputButton.WeakAttack);
            if (ctx.canceled) OnButtonUp(InputButton.WeakAttack);
        }

        /// <summary>
        /// 強攻撃
        /// </summary>
        /// <param name="ctx"></param>
        public void OnStrongAttack(InputAction.CallbackContext ctx)
        {
            if (ctx.started) OnButtonDown(InputButton.StrongAttack);
            if (ctx.canceled) OnButtonUp(InputButton.StrongAttack);
        }
    }
}