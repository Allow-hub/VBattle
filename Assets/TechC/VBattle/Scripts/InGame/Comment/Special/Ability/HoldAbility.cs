using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Util;
using TechC.VBattle.Systems;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace TechC.VBattle.InGame.Comment
{
    /// <summary>
    /// オブジェクトを手に持つアビリティ
    /// </summary>
    public class HoldAbility : ICommentAbility
    {
        [SerializeField] private GameObject gameObject;

        public void Init(SpecialCommentTrigger trigger) { }

        public void Release() { }

        public void OnTriggerEnter(Collider collider)
        {
            var characterController = collider.GetComponent<TechC.VBattle.InGame.Character.CharacterController>();

            GameObject obj = EffectFactory.I.GetEffectObj(
                gameObject,
                characterController.HandPos.position,
                Quaternion.identity
            );

            characterController.SetHoldItem(obj);
            AttachToHand(obj, characterController.HandPos);
        }

        /// <summary>
        /// オブジェクトを手に装着する
        /// </summary>
        private async void AttachToHand(GameObject obj, Transform handTransform)
        {
            obj.transform.SetParent(handTransform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }
    }
}