using UnityEngine;

namespace TechC.VBattle.InGame.Comment
{
    /// <summary>
    /// Playerの攻撃力を上昇させるバフ
    /// </summary>
    public class AttackBuff : BuffBase
    {
        private const float DEFAULT_ATTACK_MULTIPLIER = 1.5f;
        private const float DEFAULT_BUFF_DURATION = 5.0f;
        
        [SerializeField] private float attackMultiplier = DEFAULT_ATTACK_MULTIPLIER; /*攻撃力上昇の倍率 */

        public AttackBuff()
        {
            buffName = "AttackBuff";
            description = "攻撃力が上昇する";
            buffDuration = DEFAULT_BUFF_DURATION;
            remainingTime = buffDuration;
        }

        /// <summary>
        /// 攻撃力上昇のバフを適用する
        /// </summary>
        /// <param name="target"></param>
        public override void Apply(GameObject target)
        {
            Character.CharacterController characterController = target.GetComponent<Character.CharacterController>();
            if (characterController != null)
                characterController.AddMultiplier(BuffType.Attack, id, attackMultiplier);
        }

        /// <summary>
        /// 攻撃力上昇のバフを解除する
        /// </summary>
        /// <param name="target"></param>
        public override void Remove(GameObject target)
        {
            Character.CharacterController characterController = target.GetComponent<Character.CharacterController>();
            if (characterController != null)
                characterController.RemoveMultiplier(BuffType.Attack, id, attackMultiplier);
        }
    }
}
