using System.Threading;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    public class AttackDamageDetector : IAttackBehaviour
    {
        [SerializeField] private AttackData attackData;
        [SerializeField] private Collider col;
        private float currnetTime;
        private GameObject ownerObj;
        private GameObject character;
        private CancellationTokenSource attackCTS;
        private AttackObjectController attackObjectController;

        public void Initialize(GameObject owner)
        {
            ownerObj = owner;
            col.enabled = false;
            attackObjectController = owner.GetComponent<AttackObjectController>();
        }
        public void Activate(GameObject character)
        {
            if (col == null) return;
            col.enabled = false;
            this.character = character;
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
            if (!other.gameObject.CompareTag(attackObjectController.PlayerTag)) return;
            var characterController = other.transform.root.GetComponent<CharacterController>();
            if (characterController == null) return;
            // if (characterController.PlayerID == attackObjectController.PlayerID) return;// 自分自身への接触は無視
            // AttackProcessor_Refacta.ProcessAttack(attackData, character.GetComponent<CharacterController>(), ownerObj, attackCTS.Token);
        }
    }
}
