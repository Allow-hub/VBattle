using System;
using UnityEngine;

namespace TechC.CommentSystem
{
    /// <summary>
    /// Playerの移動速度を上昇させるバフ
    /// </summary>
    [Serializable]
    public class SpeedBuff : BuffBase
    {
        [SerializeField] private float speedMultiplier = 1.5f;
        public SpeedBuff()
        {
            buffName = "SpeedBuff";
            description = "移動速度が上昇する";
            buffDuration = 5.0f;
            remainingTime = buffDuration;
        }

        /// <summary>
        /// 移動速度上昇のバフを適用する
        /// </summary>
        /// <param name="target"></param>
        public override void Apply(GameObject target)
        {
            VBattle.InGame.Character.CharacterController characterController = target.GetComponent<VBattle.InGame.Character.CharacterController>();

            if (characterController != null)
                characterController.AddMultiplier(BuffType.Speed, id, speedMultiplier);
        }

        /// <summary>
        /// 移動速度上昇のバフを解除する
        /// </summary>
        /// <param name="target"></param>
        public override void Remove(GameObject target)
        {
            VBattle.InGame.Character.CharacterController characterController = target.GetComponent<VBattle.InGame.Character.CharacterController>();

            if (characterController != null)
                characterController.RemoveMultiplier(BuffType.Speed, id, speedMultiplier);
        }
    }
}
