using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    public class JumpCommand : ICommand
    {
        private CharacterController characterController;
        public JumpCommand(CharacterController controller)
        {
            characterController = controller;
        }
        

        public CommandType Type => CommandType.Jump;

        public void Execute()
        {
            characterController.Jump();
        }
    }
}
