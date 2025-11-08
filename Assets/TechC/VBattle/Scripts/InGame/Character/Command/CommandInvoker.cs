using System.Collections.Generic;
using TechC.VBattle.InGame.Input;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// コマンド実行管理（入力→解析→即時実行）
    /// 上攻撃とジャンプの同時入力対策済み
    /// Weak/Strong両方対応
    /// </summary>
    public class CommandInvoker
    {
        private const int SnapshotBufferSize = 10; // 入力履歴保持フレーム数
        private const int ComboWindow = 5;         // コンボ判定ウィンドウ（フレーム）

        private readonly CharacterController controller;
        private readonly BaseInputManager baseInput;
        private readonly List<BaseInputManager.InputSnapshot> snapHistory = new();

        private bool suppressNextJumpRelease = false;
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

            // 入力スナップ取得
            var snap = baseInput.ConsumeSnapshot(frame);
            snapHistory.Add(snap);
            if (snapHistory.Count > SnapshotBufferSize)
                snapHistory.RemoveAt(0);

            // --- 攻撃判定 ---
            BaseInputManager.InputButton attackButton = BaseInputManager.InputButton.None;
            AttackType attackType = AttackType.None;

            if ((snap.pressedButtons & BaseInputManager.InputButton.WeakAttack) != 0)
            {
                attackButton = BaseInputManager.InputButton.WeakAttack;
                attackType = AttackType.Weak;
            }
            else if ((snap.pressedButtons & BaseInputManager.InputButton.StrongAttack) != 0)
            {
                attackButton = BaseInputManager.InputButton.StrongAttack;
                attackType = AttackType.Strong;
            }

            if (attackButton != BaseInputManager.InputButton.None)
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
                {
                    ExecuteCommand(new AttackCommand(controller, attackType, AttackDirection.Upper));
                    suppressNextJumpRelease = true; // 上攻撃の時は次のジャンプリリースを無視
                }
                else if (Mathf.Abs(snap.x) > 0.5f)
                {
                    ExecuteCommand(new AttackCommand(controller, attackType, snap.x > 0 ? AttackDirection.Forward : AttackDirection.Back));
                }
                else
                {
                    ExecuteCommand(new AttackCommand(controller, attackType, AttackDirection.Neutral));
                }
            }

            // --- ジャンプ判定 ---
            bool jumpReleased = (snap.releasedButtons & BaseInputManager.InputButton.Jump) != 0;
            if (jumpReleased)
            {
                if (!suppressNextJumpRelease)
                {
                    ExecuteCommand(new JumpCommand(controller));
                }
                else
                {
                    suppressNextJumpRelease = false; // 無視したらリセット
                }
            }

            // --- 移動判定 ---
            if (attackButton == BaseInputManager.InputButton.None)
            {
                var moveThreshold = 0.01f;
                if (Mathf.Abs(snap.x) > moveThreshold || Mathf.Abs(snap.y) > moveThreshold)
                {
                    ExecuteCommand(new MoveCommand(controller, new Vector2(snap.x, snap.y)));
                }
            }

            frame++;
        }

        private void ExecuteCommand(ICommand command)
        {
            controller.ExecuteCommand(command);
        }
    }
}