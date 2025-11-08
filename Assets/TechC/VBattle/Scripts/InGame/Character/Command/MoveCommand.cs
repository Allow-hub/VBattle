using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TechC.VBattle.InGame;

namespace TechC.VBattle.InGame.Character
{
    public class MoveCommand : ICommand
    {
        private readonly CharacterController owner;
        private readonly Vector2 dir;
        public MoveCommand(CharacterController owner, UnityEngine.Vector2 dir)
        {
            this.owner = owner;
            this.dir = dir;
        }
        
        public bool IsFinished => true;
        public void Execute()
        {
            owner.Move(dir);
        }
        public void Undo() { }
        public void ForceFinish() { }
    }
}
