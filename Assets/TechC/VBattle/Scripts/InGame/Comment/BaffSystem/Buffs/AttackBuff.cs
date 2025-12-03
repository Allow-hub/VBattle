using UnityEngine;

namespace TechC.VBattle.InGame.Comment
{
    /// <summary>
    /// Playerの攻撃力を上昇させるバフ
    /// </summary>
    public class AttackBuff : BuffBase
    {
        [SerializeField] private float attackMultiplier = 1.5f; /*攻撃力上昇の倍率 */

        public AttackBuff()
        {
            buffName = "AttackBuff";
            description = "攻撃力が上昇する";
            buffDuration = 5.0f;
            remainingTime = buffDuration;
        }

        /// <summary>
        /// 攻撃力上昇のバフを適用する
        /// </summary>
        /// <param name="target"></param>
        public override void Apply(GameObject target)
        {
            VBattle.InGame.Character.CharacterController characterController = target.GetComponent<VBattle.InGame.Character.CharacterController>();
            if (characterController != null)
                characterController.AddMultiplier(BuffType.Attack, id, attackMultiplier);
        }

        /// <summary>
        /// 攻撃力上昇のバフを解除する
        /// </summary>
        /// <param name="target"></param>
        public override void Remove(GameObject target)
        {
            VBattle.InGame.Character.CharacterController characterController = target.GetComponent<VBattle.InGame.Character.CharacterController>();
            if (characterController != null)
                characterController.RemoveMultiplier(BuffType.Attack, id, attackMultiplier);
        }
    }
}
