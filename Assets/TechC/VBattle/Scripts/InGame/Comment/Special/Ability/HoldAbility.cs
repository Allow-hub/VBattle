using UnityEngine;

namespace TechC.CommentSystem
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
            var characterController = collider.GetComponentInParent<Player.CharacterController>();
            if (characterController.HoldItem != null) return;

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
        private void AttachToHand(GameObject obj, Transform handTransform)
        {
            obj.transform.SetParent(handTransform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }
    }
}