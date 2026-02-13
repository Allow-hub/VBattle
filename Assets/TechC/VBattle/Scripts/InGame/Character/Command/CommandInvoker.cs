using System;
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

        // 攻撃コマンド記録用
        private struct AttackCommandRecord
        {
            public int frame;
            public AttackType attackType;
            public AttackDirection direction;
        }
        private readonly List<AttackCommandRecord> attackCommandHistory = new();
        private const int AttackHistoryDurationFrames = 600; // 約10秒分（60fps想定）

        // 攻撃コマンド除外設定
        [System.Serializable]
        public class AttackFilter
        {
            public List<AttackType> excludedAttackTypes = new();
            public List<AttackDirection> excludedDirections = new();
            public bool excludeAllSpecial = false; // 必殺技を全て除外

            /// <summary>
            /// 指定された攻撃が除外対象かチェック
            /// </summary>
            public bool ShouldExclude(AttackType attackType, AttackDirection direction)
            {
                if (excludedAttackTypes.Contains(attackType))
                    return true;
                
                if (excludedDirections.Contains(direction))
                    return true;
                
                return false;
            }

            /// <summary>
            /// 特定の組み合わせのみを除外
            /// </summary>
            public bool ShouldExcludeCombination(AttackType attackType, AttackDirection direction)
            {
                // 例: 弱攻撃 + 上方向のみ除外
                return excludedAttackTypes.Contains(attackType) && excludedDirections.Contains(direction);
            }
        }

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

            // 古い攻撃履歴を削除
            CleanupOldAttackHistory();

            // --- ガード ---
            CheckGuardInput(latestSnap);

            // --- 必殺技 ---
            CheckSpecialInput(latestSnap);
            
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
        /// 古い攻撃履歴を削除
        /// </summary>
        private void CleanupOldAttackHistory()
        {
            int threshold = frame - AttackHistoryDurationFrames;
            attackCommandHistory.RemoveAll(record => record.frame < threshold);
        }

        /// <summary>
        /// 指定秒数前からの攻撃コマンドを再生（フィルタなし）
        /// </summary>
        /// <param name="durationSeconds">何秒前から再生するか</param>
        /// <param name="targetController">再生先のコントローラー</param>
        public void ReplayAttackCommandsFromSecondsAgo(float durationSeconds, CharacterController targetController)
        {
            ReplayAttackCommandsFromSecondsAgo(durationSeconds, targetController, (AttackFilter)null);
        }

        /// <summary>
        /// 指定秒数前からの攻撃コマンドを再生（フィルタ付き）
        /// </summary>
        /// <param name="durationSeconds">何秒前から再生するか</param>
        /// <param name="targetController">再生先のコントローラー</param>
        /// <param name="filter">除外設定（nullの場合は全て再生）</param>
        public void ReplayAttackCommandsFromSecondsAgo(float durationSeconds, CharacterController targetController, AttackFilter filter)
        {
            int frameOffset = Mathf.RoundToInt(durationSeconds * 60f); // 60fps想定
            int startFrame = frame - frameOffset;

            foreach (var record in attackCommandHistory)
            {
                if (record.frame >= startFrame)
                {
                    // フィルタチェック
                    if (filter != null && filter.ShouldExclude(record.attackType, record.direction))
                        continue; // 除外対象の場合はスキップ

                    int delayFrames = record.frame - startFrame;
                    float delaySeconds = delayFrames / 60f;
                    
                    // 遅延実行で攻撃コマンドを再生
                    DelayedExecuteAttackCommand(targetController, record.attackType, record.direction, delaySeconds);
                }
            }
        }

        /// <summary>
        /// カスタム条件で攻撃コマンドを再生
        /// </summary>
        /// <param name="durationSeconds">何秒前から再生するか</param>
        /// <param name="targetController">再生先のコントローラー</param>
        /// <param name="predicate">カスタムフィルタ条件（trueで再生、falseで除外）</param>
        public void ReplayAttackCommandsFromSecondsAgo(float durationSeconds, CharacterController targetController, 
            Func<AttackType, AttackDirection, bool> predicate)
        {
            int frameOffset = Mathf.RoundToInt(durationSeconds * 60f);
            int startFrame = frame - frameOffset;

            foreach (var record in attackCommandHistory)
            {
                if (record.frame >= startFrame)
                {
                    // カスタム条件チェック
                    if (predicate != null && !predicate(record.attackType, record.direction))
                        continue; // 条件に合わない場合はスキップ

                    int delayFrames = record.frame - startFrame;
                    float delaySeconds = delayFrames / 60f;
                    
                    DelayedExecuteAttackCommand(targetController, record.attackType, record.direction, delaySeconds);
                }
            }
        }

        /// <summary>
        /// 遅延して攻撃コマンドを実行
        /// </summary>
        private async void DelayedExecuteAttackCommand(CharacterController targetController, AttackType attackType, AttackDirection direction, float delaySeconds)
        {
            if (delaySeconds > 0)
                await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(delaySeconds * 1000));
            
            if (targetController != null)
                targetController.ExecuteCommand(new AttackCommand(attackType, direction));
        }

        /// <summary>
        /// ガード入力チェック
        /// </summary>
        private void CheckGuardInput(BaseInputManager.InputSnapshot snap)
        {
            bool guardHolding = (snap.holdButtons & BaseInputManager.InputButton.Guard) != 0;
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
            else if (!guardHolding && isGuarding)
            {
                controller.ExecuteCommand(new GuardCommand(false));
                isGuarding = false;
            }
        }

        /// <summary>
        /// 必殺技の入力チェック
        /// </summary>
        private void CheckSpecialInput(BaseInputManager.InputSnapshot snap)
        {
            bool specialPressed = (snap.pressedButtons & BaseInputManager.InputButton.Special) != 0;
            if(specialPressed)
                controller.ExecuteCommand(new SpecialCommand());
        }
        
        /// <summary>
        /// 攻撃の入力チェック
        /// </summary>
        private void CheckAttackInput(BaseInputManager.InputSnapshot snap)
        {
            AttackType attackType = AttackType.None;

            if ((snap.pressedButtons & BaseInputManager.InputButton.WeakAttack) != 0)
                attackType = AttackType.Weak;
            else if ((snap.pressedButtons & BaseInputManager.InputButton.StrongAttack) != 0)
                attackType = AttackType.Strong;

            if (attackType == AttackType.None)
                return;

            AttackDirection direction = DetermineAttackDirection(snap);
            controller.ExecuteCommand(new AttackCommand(attackType, direction));
            RecordAttackCommand(attackType, direction);

            if (direction == AttackDirection.Up)
                suppressNextJumpRelease = true;
        }

        /// <summary>
        /// 攻撃コマンドを履歴に記録
        /// </summary>
        private void RecordAttackCommand(AttackType attackType, AttackDirection direction)
        {
            attackCommandHistory.Add(new AttackCommandRecord
            {
                frame = frame,
                attackType = attackType,
                direction = direction
            });
        }

        /// <summary>
        /// 攻撃派生を確定する
        /// </summary>
        private AttackDirection DetermineAttackDirection(BaseInputManager.InputSnapshot snap)
        {
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
        private void CheckCrouchInput(BaseInputManager.InputSnapshot snap)
        {
            bool crouchHolding = (snap.holdButtons & BaseInputManager.InputButton.Crouch) != 0;
            bool crouchPressed = (snap.pressedButtons & BaseInputManager.InputButton.Crouch) != 0;
            bool crouchReleased = (snap.releasedButtons & BaseInputManager.InputButton.Crouch) != 0;

            if (crouchPressed && !isCrouching)
            {
                controller.ExecuteCommand(new CrouchCommand(true));
                isCrouching = true;
            }
            else if (crouchReleased && isCrouching)
            {
                controller.ExecuteCommand(new CrouchCommand(false));
                isCrouching = false;
            }
            else if (!crouchHolding && isCrouching)
            {
                controller.ExecuteCommand(new CrouchCommand(false));
                isCrouching = false;
            }
        }

        /// <summary>
        /// 移動入力
        /// </summary>
        private void CheckMoveInput(BaseInputManager.InputSnapshot snap)
        {
            bool moveHolding = (snap.holdButtons & BaseInputManager.InputButton.Move) != 0;
            const float moveThreshold = 0.2f;

            float x = snap.x;

            if (!moveHolding || Mathf.Abs(x) <= moveThreshold)
            {
                lastMoveReleased = true;
                isDashing = false;
                controller.Anim.SetBool(AnimatorParam.IsMoving, false);
                return;
            }

            float currentDir = x > 0 ? 1f : -1f;
            bool dash = false;

            if (lastMoveReleased)
            {
                int delta = frame - lastMoveFrame;
                if (currentDir == lastMoveDir && delta <= DashInputWindow)
                {
                    dash = true;
                    isDashing = true;
                }
                controller.Anim.SetBool(AnimatorParam.IsMoving, true);
            }
            else if (isDashing)
                dash = true;

            controller.ExecuteCommand(new MoveCommand(new Vector2(currentDir, 0), dash));
            HasMoveInput = true;
            lastMoveDir = currentDir;
            lastMoveFrame = frame;
            lastMoveReleased = false;
        }
    }
}