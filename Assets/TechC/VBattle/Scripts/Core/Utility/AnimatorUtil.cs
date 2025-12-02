using UnityEngine;

namespace TechC.VBattle.Core.Util
{
    public static class AnimatorUtil
    {
        /// <summary>
        /// AnimatorのすべてのBoolをOFFにして、指定したBoolだけONにする（ハッシュ版）
        /// </summary>
        /// <param name="anim">対象Animator</param>
        /// <param name="onBoolHash">ONにしたいBoolのハッシュ</param>
        public static void SetAnimatorBoolExclusive(Animator anim, int onBoolHash)
        {
            if (anim == null) return;

            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Bool)
                {
                    int paramHash = Animator.StringToHash(param.name);
                    anim.SetBool(paramHash, paramHash == onBoolHash);
                }
            }
        }
    }
}