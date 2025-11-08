using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    public class GuardCommand : ICommand
    {
        private readonly bool isPress; // true: ガード開始, false: ガード解除

        public GuardCommand(bool isPress = true)
        {
            this.isPress = isPress;
        }

        public CommandType Type => CommandType.Guard;

        public void Execute()
        {
            // ガード処理はCharacterController側で状態遷移として処理
            // ここでは何もしない（状態遷移のみ）
        }

        public bool IsPress() => isPress;
    }
}
