using System;
using UnityEngine;

namespace TechC.VBattle.InGame.Input
{
    /// <summary>
    /// キャラクター入力を管理する抽象基底クラス
    /// PlayerとNpcの共通処理を定義します（入力受付のみ）
    /// </summary>
    public class BaseInputManager : MonoBehaviour
    {
        [SerializeField] protected Character.CharacterController characterController;

        [Flags]
        public enum InputButton : uint
        {
            None = 0,
            WeakAttack = 1 << 0,
            StrongAttack = 1 << 1,
            Jump = 1 << 2,
            Guard = 1 << 3,
        }

        /// <summary>
        /// 入力スナップショット（1フレーム分）
        /// </summary>
        public struct InputSnapshot
        {
            public float x; // -1..1
            public float y; // -1..1
            public InputButton holdButtons;    // 現在ホールド中のボタン（押しっぱなし）
            public InputButton pressedButtons; // 押した瞬間
            public InputButton releasedButtons; // 離した瞬間
            public int frame;
        }

        // --- 内部保持（派生から直接更新可能） ---
        protected float holdX = 0f;
        protected float holdY = 0f;
        protected InputButton holdButtons = InputButton.None;
        protected InputButton pressedButtons = InputButton.None;
        protected InputButton releasedButtons = InputButton.None;

        // --- 入力更新 API（PlayerInputManager などが呼ぶ） ---
        public void SetMove(Vector2 v)
        {
            holdX = Mathf.Clamp(v.x, -1f, 1f);
            holdY = Mathf.Clamp(v.y, -1f, 1f);
        }

        public void OnButtonDown(InputButton b)
        {
            if ((holdButtons & b) == 0) // すでに押していたら無視
            {
                pressedButtons |= b;
                holdButtons |= b;
            }
        }

        public void OnButtonUp(InputButton b)
        {
            if ((holdButtons & b) != 0)
            {
                releasedButtons |= b;
                holdButtons &= ~b;
            }
        }

        /// <summary>
        /// CommandInvoker が毎フレーム呼んでスナップを取得する
        /// </summary>
        public InputSnapshot ConsumeSnapshot(int currentFrame = 0)
        {
            var s = new InputSnapshot
            {
                x = holdX,
                y = holdY,
                holdButtons = holdButtons,
                pressedButtons = pressedButtons,
                releasedButtons = releasedButtons,
                frame = currentFrame
            };

            // ワンフレームフラグをリセット
            pressedButtons = InputButton.None;
            releasedButtons = InputButton.None;

            return s;
        }
    }
}