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

        public void Init(SpecialCommentTrigger trigger)
        {
        }

        public void Release() { }

        public void OnTriggerEnter(Collider collider)
        {
            if (CommentDisplay.I.IsCommentFrozen) return;

            var characterController = collider.transform.root.GetComponent<TechC.VBattle.InGame.Character.CharacterController>();
            if (characterController == null) return;
            if (characterController.HoldItem == null) return;

            RegisterThrowEvent(characterController);

        }

        /// <summary>
        /// 投げるイベントを登録する
        /// </summary>
        private void RegisterThrowEvent(TechC.VBattle.InGame.Character.CharacterController characterController)
        {
            characterController.RegisterCommentEvent(() =>
            {
                Throw(characterController.HoldItem.GetComponent<Rigidbody>(), characterController.HoldItem);
            });
        }

        public void Throw(Rigidbody rb, GameObject commentObj)
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;
            rb.useGravity = true;
            var character = commentObj.transform.root;
            var dirZ = UnityEngine.Random.Range(throwUpwardPower.x, throwUpwardPower.y);
            Vector3 throwDirection = (character.forward + Vector3.up * dirZ).normalized;
            rb.velocity = Vector3.zero;
            rb.AddForce(throwDirection * throwPower, ForceMode.Impulse);
            commentObj.transform.SetParent(null);
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

            var characterController = character.GetComponent<TechC.VBattle.InGame.Character.CharacterController>();
            if (characterController == null) return;
            characterController.SetHoldItem(null);

        }
    }
}
