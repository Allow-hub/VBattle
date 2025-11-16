using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 移動コマンド、移動方向の情報を持つ
    /// </summary>
    public struct MoveCommand : ICommand
    {
        public CommandType Type => CommandType.Move;
        public Vector2 Dir { get; }
        
        public MoveCommand(Vector2 dir)
        {
            Dir = dir;
        }
    }
}
