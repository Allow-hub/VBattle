using UnityEngine;

namespace TechC.VBattle.InGame.Events
{
    public struct AttackEvent : IBattleEvent
    {
        public CharacterController Attacker;
        public CharacterController Target;
        // public AttackData AttackData;
        public Vector3 HitPosition;
    }
}