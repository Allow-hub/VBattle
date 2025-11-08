using System.Collections;
using System.Collections.Generic;
using TechC.VBattle.InGame;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    public enum AttackType
    {
        None,
        Weak,
        Strong,
        Air,
    }
    public enum AttackDirection
    {
        Neutral,    // 通常攻撃
        Forward,    // 前攻撃
        Back,       // 後ろ攻撃
        Upper,      // 上攻撃
        Downer,     // 下攻撃
    }

    public class AttackCommand : ICommand
    {
        private readonly CharacterController owner;
        private readonly AttackType attackType;
        private readonly AttackDirection attackDirection;

        public AttackCommand(CharacterController owner, AttackType attackType, AttackDirection attackDirection)
        {
            this.owner = owner;
            this.attackType = attackType;
            this.attackDirection = attackDirection;
        }

        public bool IsFinished => true;
        public void Execute()
        {
            Debug.Log($"Attack Command Executed: {attackType}");
        }
        public void Undo() { }
        public void ForceFinish() { }
    }
}