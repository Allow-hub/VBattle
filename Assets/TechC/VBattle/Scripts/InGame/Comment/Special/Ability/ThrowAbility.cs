using UnityEngine;
using System;

namespace TechC.VBattle.InGame.Comment
{
    /// <summary>
    /// オブジェクトを投げるアビリティ
    /// </summary>
    [Serializable]

    public class ThrowAbility : ICommentAbility
    {
        [SerializeField] private float throwPower = 10f;
        [SerializeField] private Vector2 throwUpwardPower = new Vector2(0.5f, 1.0f);

        public void Init(SpecialCommentTrigger trigger) { }

        public void Release() { }

        public void OnTriggerEnter(Collider collider)
        {
            if (CommentDisplay.I.IsCommentFrozen) return;

            var characterController = collider.transform.root.GetComponent<TechC.VBattle.InGame.Character.CharacterController>();
            if (characterController == null) return;

            // CommentAbilityHandlerを経由してアビリティを登録
            RegisterThrowAbility(characterController.CommentAbilityHandler, characterController);
        }

        /// <summary>
        /// 投げるアビリティを登録する
        /// </summary>
        private void RegisterThrowAbility(CommentAbilityHandler abilityHandler, TechC.VBattle.InGame.Character.CharacterController characterController)
        {
            var holdItem = characterController.HoldItem;
            if (holdItem == null) return;
            
            abilityHandler.RegisterAbility(() =>
            {
                ThrowItem(holdItem, characterController);
            });
        }

        private void ThrowItem(GameObject item, TechC.VBattle.InGame.Character.CharacterController character)
        {
            if (item == null || character == null) return;

            var rb = item.GetComponent<Rigidbody>();

            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;
            rb.useGravity = true;

            var dirZ = UnityEngine.Random.Range(throwUpwardPower.x, throwUpwardPower.y);
            Vector3 throwDirection = (character.transform.forward + Vector3.up * dirZ).normalized;
            
            rb.velocity = Vector3.zero;
            rb.AddForce(throwDirection * throwPower, ForceMode.Impulse);
            
            item.transform.SetParent(null);
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

            character.SetHoldItem(null);
        }
    }
}
