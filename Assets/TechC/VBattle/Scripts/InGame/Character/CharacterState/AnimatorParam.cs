using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// アニメーションのパラメーター
    /// </summary>
    public static class AnimatorParam
    {
        public static readonly int IsMoving = Animator.StringToHash("IsMoving");
        public static readonly int IsJumping = Animator.StringToHash("IsJumping");
        public static readonly int IsGuarding = Animator.StringToHash("IsGuarding");
        public static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
        public static readonly int IsHitting = Animator.StringToHash("IsHitting");
        public static readonly int IsHiIsWallHittingtting = Animator.StringToHash("IsWallHitting");
        public static readonly int XSpeed = Animator.StringToHash("XSpeed");
        public static readonly int YSpeed = Animator.StringToHash("YSpeed");
        public static readonly int AttackType = Animator.StringToHash("AttackType");
        public static readonly int AttackDirection = Animator.StringToHash("AttackDirection");
    }
}