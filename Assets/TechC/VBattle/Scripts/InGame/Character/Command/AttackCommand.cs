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
        private readonly AttackType attackType;
        private readonly AttackDirection attackDirection;

        public AttackCommand(AttackType attackType, AttackDirection attackDirection)
        {
            this.attackType = attackType;
            this.attackDirection = attackDirection;
        }
        public CommandType Type => CommandType.Attack;

        public void Execute()
        {
        }

        public AttackType GetAttackType() => attackType;
        public AttackDirection GetAttackDirection() => attackDirection;
    }
}