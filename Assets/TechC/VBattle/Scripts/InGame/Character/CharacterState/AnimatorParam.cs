using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// アニメーションのパラメーター
    /// </summary>
    public static class AnimatorParam
    {
        public static int IsWalking = Animator.StringToHash("IsWalking");
        public static int IsDashing = Animator.StringToHash("IsDashing");
        public static int IsJumping = Animator.StringToHash("IsJumping");
        public static int IsGuarding = Animator.StringToHash("IsGuarding");
    }
}