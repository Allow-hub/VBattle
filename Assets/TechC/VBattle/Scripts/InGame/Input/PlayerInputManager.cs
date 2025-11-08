using TechC.VBattle.InGame.Character;
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

        public void OnDown(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) { holdY = -1f; SetMove(new Vector2(holdX, holdY)); }
            if (ctx.canceled) { if (holdY < 0f) holdY = 0f; SetMove(new Vector2(holdX, holdY)); }
        }

        public void OnWeakAttack(InputAction.CallbackContext ctx)
        {
            if (ctx.started) OnButtonDown(InputButton.WeakAttack);
            if (ctx.canceled) OnButtonUp(InputButton.WeakAttack);
        }

        public void OnStrongAttack(InputAction.CallbackContext ctx)
        {
            if (ctx.started) OnButtonDown(InputButton.StrongAttack);
            if (ctx.canceled) OnButtonUp(InputButton.StrongAttack);
        }
    }
}