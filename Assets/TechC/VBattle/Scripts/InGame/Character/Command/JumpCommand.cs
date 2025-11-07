using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    public class JumpCommand : ICommand
    {
        public bool IsFinished => false;

        public void Execute()
        {
        }
        public void Undo()
        {
        }
        public void ForceFinish()
        {
        }

    }
}
