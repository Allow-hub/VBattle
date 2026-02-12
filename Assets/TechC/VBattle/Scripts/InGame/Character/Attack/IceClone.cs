using TechC.VBattle.Core.Util;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 自分の攻撃を再現するふるまい
    /// </summary>
    public class IceClone : IAttackBehaviour
    {
        [SerializeField] private GameObject iceClonePrefab;
        [SerializeField] private AttackData attackData;
        private GameObject iceCloneObj;
        [SerializeField] private GameObject ownerObj;
        [SerializeField] private GameObject magicCircle;
        [SerializeField] private float magicDuration = 0.5f;
        [SerializeField] private float magicYOffset = 0.5f;
        private float elapsedTime = 0;
        private GameObject characterObj;
        public void Initialize(GameObject owner)
        {
        }

        public void OnRelease()
        {
            Object.Destroy(iceCloneObj);
        }

        public void OnUpdate(float deltaTime)
        {
            elapsedTime += deltaTime;
            if (magicCircle == null && elapsedTime >= magicDuration)
            {
                magicCircle.SetActive(false);
                return;
            }
            if (characterObj == null || magicCircle == null) return;
            var pos = new Vector3(characterObj.transform.position.x, characterObj.transform.position.y + magicYOffset, characterObj.transform.position.z);
            magicCircle.transform.position = pos;
        }

        public void Activate(GameObject character)
        {
            var characterController = character.GetComponent<CharacterController>();
            var transformRecorder = character.GetComponent<TransformRecorder>();
            var commandInvorker = characterController.CommandInvoker;
            magicCircle.SetActive(true);
            characterObj = character;
            magicCircle.transform.position = character.transform.position;

            DelayUtility.StartDelayedActionWithPauseAsync(attackData.hitTiming, () =>
            {
                iceCloneObj = Object.Instantiate(iceClonePrefab);
                iceCloneObj.transform.SetParent(ownerObj.transform);
                transformRecorder.StartReplayFromSecondsAgo(attackData.attackDuration, iceCloneObj.transform);
                var cloneController = iceCloneObj.GetComponent<CharacterController>();
                cloneController.SetClonePlayerID(characterController.PlayerIndex);
                commandInvorker.ReplayAttackCommandsFromSecondsAgo(attackData.attackDuration, cloneController,
                    (attackType, direction) => !(attackType == AttackType.Strong && direction == AttackDirection.Neutral));
            }, InGameManager.I?.GetPauseStateFunc);
        }
    }
}