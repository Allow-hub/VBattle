using System.Threading;
using TechC.VBattle.InGame.Events;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 飛び道具の当たり判定とダメージ判定
    /// </summary>
    public class AttackDamageDetector : IAttackBehaviour, IAttacker
    {
        [SerializeField] private AttackData attackData;
        [SerializeField] private Collider col;
        private float currnetTime;
        private CancellationTokenSource attackCTS;

        // --- IAttackerの継承　--- ///
        public GameObject AttackerObj { get; private set; }
        public Transform Transform => AttackerObj.transform;
        public CharacterController Owner { get; private set; }

        public void Initialize(GameObject owner)
        {
            AttackerObj = owner;
            col.enabled = false;
        }

        public void Activate(GameObject character)
        {
            if (col == null) return;
            Owner = character.gameObject.transform.root.GetComponent<CharacterController>();
            col.enabled = false;
            currnetTime = 0f;
            attackCTS = new CancellationTokenSource();
        }

        public void OnRelease()
        {
            if (col == null) return;
            col.enabled = false;
            if (attackCTS == null) return;
            attackCTS.Cancel();
            attackCTS.Dispose();
            attackCTS = null;
        }

        public void OnUpdate(float deltaTime)
        {
            if (currnetTime >= attackData.hitTiming)
            {
                if (col == null) return;
                col.enabled = true;
            }
            currnetTime += deltaTime;
        }

        public void OnTriggerEnter(Collider other)
        {
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
