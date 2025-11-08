using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TechC.VBattle.InGame;

namespace TechC.VBattle.InGame.Character
{
    public class MoveCommand : ICommand
    {
        private readonly Vector2 dir;
        public MoveCommand(Vector2 dir)
        {
            this.dir = dir;
        }
        public CommandType Type => CommandType.Move;

        public void Execute()
        {
        }
    }
}
