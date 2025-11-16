using System.Collections.Generic;
using TechC.VBattle.InGame.Input;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 入力を受け取ってCommandに変換するだけ（ロジックなし）
    /// </summary>
    public class CommandInvoker
    {
        private const int SnapshotBufferSize = 10;
        private const int ComboWindow = 5;

        private readonly CharacterController controller;
        private readonly BaseInputManager baseInput;
        private readonly List<BaseInputManager.InputSnapshot> snapHistory = new();

        private bool suppressNextJumpRelease = false;
        private bool isGuarding = false;
        private int frame = 0;

        public CommandInvoker(CharacterController controller)
        {
            this.controller = controller;
            this.baseInput = controller.GetComponent<BaseInputManager>();
        }

        public void Update()
        {
            if (baseInput == null)
                return;

            var snap = baseInput.ConsumeSnapshot(frame);
            snapHistory.Add(snap);
            if (snapHistory.Count > SnapshotBufferSize)
                snapHistory.RemoveAt(0);

            // --- ガード ---
            CheckGuardInput(snap);

            // --- 攻撃 ---
            CheckAttackInput(snap);

            // --- ジャンプ ---
            CheckJumpInput(snap);

            // --- 移動 ---
            if (!isGuarding)
            {
                CheckMoveInput(snap);
            }

            frame++;
        }

        private void CheckGuardInput(BaseInputManager.InputSnapshot snap)
        {
            bool guardPressed = (snap.pressedButtons & BaseInputManager.InputButton.Guard) != 0;
            bool guardReleased = (snap.releasedButtons & BaseInputManager.InputButton.Guard) != 0;

            if (guardPressed && !isGuarding)
            {
                controller.ExecuteCommand(new GuardCommand(true));
                isGuarding = true;
            }
            else if (guardReleased && isGuarding)
            {
                controller.ExecuteCommand(new GuardCommand(false));
                isGuarding = false;
            }
        }

        private void CheckAttackInput(BaseInputManager.InputSnapshot snap)
        {
            AttackType attackType = AttackType.None;

            if ((snap.pressedButtons & BaseInputManager.InputButton.WeakAttack) != 0)
            {
                attackType = AttackType.Weak;
            }
            else if ((snap.pressedButtons & BaseInputManager.InputButton.StrongAttack) != 0)
            {
                attackType = AttackType.Strong;
            }

            if (attackType == AttackType.None)
                return;

            // 攻撃方向を判定
            AttackDirection direction = DetermineAttackDirection(snap);
            
            controller.ExecuteCommand(new AttackCommand(attackType, direction));

            // 上攻撃の時はジャンプ抑制
            if (direction == AttackDirection.Upper)
            {
                suppressNextJumpRelease = true;
            }
        }

        private AttackDirection DetermineAttackDirection(BaseInputManager.InputSnapshot snap)
        {
            // 最近の入力履歴から上入力をチェック
            bool recentUpInput = false;
            int startIdx = Mathf.Max(0, snapHistory.Count - ComboWindow);

            for (int i = snapHistory.Count - 1; i >= startIdx; i--)
            {
                if (snapHistory[i].y > 0.5f ||
                    (snapHistory[i].holdButtons & BaseInputManager.InputButton.Jump) != 0)
                {
                    recentUpInput = true;
                    break;
                }
            }

            if (recentUpInput)
                return AttackDirection.Upper;

            if (snap.y < -0.5f)
                return AttackDirection.Downer;

            if (Mathf.Abs(snap.x) > 0.5f)
                return snap.x > 0 ? AttackDirection.Forward : AttackDirection.Back;

            return AttackDirection.Neutral;
        }

        private void CheckJumpInput(BaseInputManager.InputSnapshot snap)
        {
            bool jumpReleased = (snap.releasedButtons & BaseInputManager.InputButton.Jump) != 0;

            if (jumpReleased)
            {
                if (!suppressNextJumpRelease)
                {
                    controller.ExecuteCommand(new JumpCommand());
                }
                else
                {
                    suppressNextJumpRelease = false;
                }
            }
        }

        private void CheckMoveInput(BaseInputManager.InputSnapshot snap)
        {
            const float moveThreshold = 0.01f;

            if (Mathf.Abs(snap.x) > moveThreshold || Mathf.Abs(snap.y) > moveThreshold)
            {
                controller.ExecuteCommand(new MoveCommand(new Vector2(snap.x, snap.y)));
            }
        }
    }
}