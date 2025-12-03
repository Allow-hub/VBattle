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

        [SerializeField] private GameObject effectInstance; /* 実際にInstantiateで生成されたエフェクトのインスタンス */

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
            Player.CharacterController characterController = target.GetComponent<Player.CharacterController>();

            /* 速度上昇のバフを適用 */
            if (characterController != null)
            {
                // Debug.Log($"<color=orange>[Apply前]</color>攻撃の倍率は{characterController.GetCurrentAttackMultiplier()}");
                characterController.AddMultiplier(BuffType.Speed, id,speedMultiplier);


                // Debug.Log($"<color=orange>[Apply後]</color>:攻撃の倍率は{characterController.GetCurrentAttackMultiplier()}");

            }
        }

        /// <summary>
        /// 移動速度上昇のバフを解除する
        /// </summary>
        /// <param name="target"></param>
        public override void Remove(GameObject target)
        {
            Player.CharacterController characterController = target.GetComponent<Player.CharacterController>();

            /* 速度上昇のバフを解除 */
            if (characterController != null)
            {
                characterController.RemoveMultiplier(BuffType.Speed, id,speedMultiplier);
            }
        }
    }
}
