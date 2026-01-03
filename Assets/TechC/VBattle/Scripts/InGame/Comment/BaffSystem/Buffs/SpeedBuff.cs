using System;
using UnityEngine;

namespace TechC.VBattle.InGame.Comment
{
    /// <summary>
    /// Playerの移動速度を上昇させるバフ
    /// </summary>
    [Serializable]
    public class SpeedBuff : BuffBase
    {
        private const float DEFAULT_SPEED_MULTIPLIER = 1.5f;
        private const float DEFAULT_BUFF_DURATION = 5.0f;
        
        public SpeedBuff()
        {
            buffName = "SpeedBuff";
            description = "移動速度が上昇する";
            buffDuration = DEFAULT_BUFF_DURATION;
            remainingTime = buffDuration;
        }

        /// <summary>
        /// 移動速度上昇のバフを適用する
        /// </summary>
        /// <param name="target"></param>
        public override void Apply(GameObject target)
        {
            Character.CharacterController characterController = target.GetComponent<Character.CharacterController>();

            if (characterController != null)
                characterController.AddMultiplier(BuffType.Speed, id, DEFAULT_SPEED_MULTIPLIER);
        }

        /// <summary>
        /// 移動速度上昇のバフを解除する
        /// </summary>
        /// <param name="target"></param>
        public override void Remove(GameObject target)
        {
            Character.CharacterController characterController = target.GetComponent<Character.CharacterController>();

            if (characterController != null)
                characterController.RemoveMultiplier(BuffType.Speed, id, DEFAULT_SPEED_MULTIPLIER);
        }
    }
}
