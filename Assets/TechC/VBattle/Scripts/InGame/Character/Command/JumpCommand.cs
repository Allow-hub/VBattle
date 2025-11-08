using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    public class JumpCommand : ICommand
    {
        private readonly CharacterController owner;
        public JumpCommand(CharacterController owner)
        {
            this.owner = owner;
        }
        
        public bool IsFinished => false;

        public void Execute()
        {
            Debug.Log("Jump Command Executed");
        }
        public void Undo()
        {
        }
        public void ForceFinish()
        {
        }

    }
}
