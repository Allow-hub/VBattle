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
        public bool HasMoveInput { get; private set; }
        private const int SnapshotBufferSize = 10;
        private const int ComboWindow = 5;

        private readonly CharacterController controller;
        private readonly BaseInputManager baseInput;
        private readonly List<BaseInputManager.InputSnapshot> snapHistory = new();

        private bool suppressNextJumpRelease = false;
        private bool isGuarding = false;
        private bool isCrouching = false;
        private int frame = 0;
        private BaseInputManager.InputSnapshot latestSnap;
        private bool isDashing = false;

        private float lastMoveDir = 0f;
        private int lastMoveFrame = -999;
        private bool lastMoveReleased = true;
        private const int DashInputWindow = 20;

        public CommandInvoker(CharacterController controller)
        {
            this.controller = controller;
            baseInput = controller.GetComponent<BaseInputManager>();
        }

        public void Update()
        {
            HasMoveInput = false;
            if (baseInput == null)
                return;

            latestSnap = baseInput.ConsumeSnapshot(frame);
            snapHistory.Add(latestSnap);
            if (snapHistory.Count > SnapshotBufferSize)
                snapHistory.RemoveAt(0);

            // --- ガード ---
            CheckGuardInput(latestSnap);

            // --- 攻撃 ---
            CheckAttackInput(latestSnap);

            // --- ジャンプ ---
            CheckJumpInput(latestSnap);

            // --- しゃがみ ---
            CheckCrouchInput(latestSnap);

            frame++;
        }

        public void FixedUpdate()
        {
            // --- 移動 ---
            if (!isGuarding)
                CheckMoveInput(latestSnap);
        }

        /// <summary>
        /// ガード入力チェック
        /// </summary>
        /// <param name="snap"></param>
        private void CheckGuardInput(BaseInputManager.InputSnapshot snap)
        {
            bool guardHolding = (snap.holdButtons & BaseInputManager.InputButton.Guard) != 0;
            bool guardPressed = (snap.pressedButtons & BaseInputManager.InputButton.Guard) != 0;
            bool guardReleased = (snap.releasedButtons & BaseInputManager.InputButton.Guard) != 0;

            // ガード開始
            if (guardPressed && !isGuarding)
            {
                controller.ExecuteCommand(new GuardCommand(true));
                isGuarding = true;
            }
            // ガード解除
            else if (guardReleased && isGuarding)
            {
                controller.ExecuteCommand(new GuardCommand(false));
                isGuarding = false;
            }
            // ガード維持（holdButtonsをチェック）
            else if (!guardHolding && isGuarding)
            {
                // 何らかの理由でholdが外れた場合の保険
                controller.ExecuteCommand(new GuardCommand(false));
                isGuarding = false;
            }
        }

        /// <summary>
        /// 攻撃の入力チェック
        /// </summary>
        /// <param name="snap"></param>
        private void CheckAttackInput(BaseInputManager.InputSnapshot snap)
        {
            AttackType attackType = AttackType.None;

            if ((snap.pressedButtons & BaseInputManager.InputButton.WeakAttack) != 0)
                attackType = AttackType.Weak;
            else if ((snap.pressedButtons & BaseInputManager.InputButton.StrongAttack) != 0)
                attackType = AttackType.Strong;

            if (attackType == AttackType.None)
                return;

            // 攻撃方向を判定
            AttackDirection direction = DetermineAttackDirection(snap);

            controller.ExecuteCommand(new AttackCommand(attackType, direction));

            // 上攻撃の時はジャンプ抑制
            if (direction == AttackDirection.Up)
                suppressNextJumpRelease = true;
        }

        /// <summary>
        /// 攻撃派生を確定する
        /// </summary>
        /// <param name="snap"></param>
        /// <returns></returns>
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
                return AttackDirection.Up;

            if (snap.y < -0.5f)
                return AttackDirection.Down;

            if (Mathf.Abs(snap.x) > 0.5f)
                return snap.x > 0 ? AttackDirection.Right : AttackDirection.Left;

            return AttackDirection.Neutral;
        }

        /// <summary>
        /// ジャンプ入力チェック
        /// </summary>
        /// <param name="snap"></param>
        private void CheckJumpInput(BaseInputManager.InputSnapshot snap)
        {
            bool jumpReleased = (snap.releasedButtons & BaseInputManager.InputButton.Jump) != 0;

            if (!jumpReleased) return;
            if (!suppressNextJumpRelease)
                controller.ExecuteCommand(new JumpCommand());
            else
                suppressNextJumpRelease = false;
        }

        /// <summary>
        /// しゃがみ入力
        /// </summary>
        /// <param name="snap"></param>
        private void CheckCrouchInput(BaseInputManager.InputSnapshot snap)
        {
            bool crouchHolding = (snap.holdButtons & BaseInputManager.InputButton.Crouch) != 0;
            bool crouchPressed = (snap.pressedButtons & BaseInputManager.InputButton.Crouch) != 0;
            bool crouchReleased = (snap.releasedButtons & BaseInputManager.InputButton.Crouch) != 0;

            // しゃがみ開始
            if (crouchPressed && !isCrouching)
            {
                controller.ExecuteCommand(new CrouchCommand(true));
                isCrouching = true;
            }
            // しゃがみ解除
            else if (crouchReleased && isCrouching)
            {
                controller.ExecuteCommand(new CrouchCommand(false));
                isCrouching = false;
            }
            // しゃがみ維持（holdButtonsをチェック）
            else if (!crouchHolding && isCrouching)
            {
                // 何らかの理由でholdが外れた場合の保険
                controller.ExecuteCommand(new CrouchCommand(false));
                isCrouching = false;
            }
        }

        /// <summary>
        /// 移動入力
        /// </summary>
        /// <param name="snap"></param>
        private void CheckMoveInput(BaseInputManager.InputSnapshot snap)
        {
            bool moveHolding = (snap.holdButtons & BaseInputManager.InputButton.Move) != 0;
            const float moveThreshold = 0.2f;

            float x = snap.x;

            // ニュートラル
            if (!moveHolding || Mathf.Abs(x) <= moveThreshold)
            {
                lastMoveReleased = true;
                isDashing = false;//話したらダッシュ解除
                controller.Anim.SetBool(AnimatorParam.IsMoving, false);
                return;
            }

            float currentDir = x > 0 ? 1f : -1f;

            bool dash = false;

            // 前回移動がリリースされていた場合に短時間連打でダッシュ判定
            if (lastMoveReleased)
            {
                int delta = frame - lastMoveFrame;
                if (currentDir == lastMoveDir && delta <= DashInputWindow)
                {
                    dash = true;
                    isDashing = true; // ダッシュ状態に
                }
                controller.Anim.SetBool(AnimatorParam.IsMoving, true); // Press時にtrue
            }
            else if (isDashing)
                dash = true;// ダッシュ状態を維持

            controller.ExecuteCommand(new MoveCommand(new Vector2(currentDir, 0), dash));
            HasMoveInput = true;
            // 状態更新
            lastMoveDir = currentDir;
            lastMoveFrame = frame;
            lastMoveReleased = false;
        }
    }
}