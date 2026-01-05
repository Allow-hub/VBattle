using System.Collections;
using System.Collections.Generic;
using TechC.VBattle.InGame.Events;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// あめ必殺技
    /// </summary>
    public class AttackAmeSpecial : IAttackBehaviour, IAttacker
    {
        [SerializeField] private AttackData attackData;

        // --- IAttackerの継承　--- ///
        public GameObject AttackerObj { get; private set; }
        public Transform Transform => AttackerObj.transform;
        public CharacterController Owner { get; private set; }

        public void Initialize(GameObject owner)
        {
            AttackerObj = owner;
        }

        public void OnRelease()
        {
        }

        public void OnUpdate(float deltaTime)
        {
        }

        public void Activate(GameObject character)
        {
            Owner = character.gameObject.transform.root.GetComponent<CharacterController>();
        }

        public void OnTriggerEnter(Collider other)
        {
            Debug.Log($"Ame Special Hit: {other.gameObject.name}");

            if (!other.gameObject.CompareTag(Owner.PlayerTag)) return;
            var characterController = other.transform.root.GetComponent<CharacterController>();
            if (characterController == null) return;
            // BattleJudgeに判定を依頼
            InGameManager.I.BattleBus.Publish(new AttackRequestEvent
            {
                attacker = this,
                attackData = attackData,
                hitPosition = other.gameObject.transform.position,
                hitTargets = new[] { other }
            });
        }
    }
}
