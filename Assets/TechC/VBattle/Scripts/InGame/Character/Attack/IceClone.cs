using UnityEngine;

namespace TechC.Player.Attack
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
        [SerializeField] private AttackObjectController attackObjectController;
        [SerializeField] private GameObject magicCircle;
        [SerializeField] private float magicDuration = 0.5f;
        private float elapsedTime = 0;
        private GameObject characterObj;
        public void Initialize(GameObject owner)
        {
        }

        public void OnRelease()
        {
            GameObject.Destroy(iceCloneObj);
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
            var pos = characterObj.transform.position.AddY(-0.5f);
            magicCircle.transform.position = pos;

        }

        public void Activate(GameObject character)
        {
            var characterController = character.GetComponent<CharacterController>();
            var transformRecorder = character.GetComponent<TransformRecorder>();
            var commandHistory = character.GetComponent<CommandHistory>();
            magicCircle.SetActive(true);
            characterObj = character;
            magicCircle.transform.position = character.transform.position;

            DelayUtility.StartDelayedActionWithPause(attackObjectController, attackData.hitTiming, BattleJudge.I.GetPauseStateFunc, () =>
            {
                iceCloneObj = GameObject.Instantiate(iceClonePrefab);
                iceCloneObj.transform.SetParent(ownerObj.transform);
                transformRecorder.StartReplayFromSecondsAgo(attackData.attackDuration, iceCloneObj.transform);

                if (commandHistory == null)
                {
                    Debug.LogWarning("CommandHistoryが見つかりませんでした");
                    return;
                }
                var cloneController = iceCloneObj.GetComponent<CharacterController>();
                cloneController.SetClonePlayerID(characterController.PlayerID);
                // if (characterController.GetCharacterState().AttackManager == null) return;
                commandHistory.ReplayAttackCommandsFromSecondsAgo(attackData.attackDuration, cloneController);
            });
        }
    }
}