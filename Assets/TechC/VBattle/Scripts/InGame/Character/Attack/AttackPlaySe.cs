using TechC.VBattle.Core.Managers;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 攻撃時のSE再生処理
    /// </summary>
    public class AttackPlaySe : IAttackBehaviour
    {
        [SerializeField] private Audio.CharacterSEType seType = Audio.CharacterSEType.None;
        public void Initialize(GameObject owner)
        {
        }

        public void OnRelease()
        {
        }

        public void OnUpdate(float deltaTime)
        {
        }

        public void Activate(GameObject character)
        {
            var charaName = GameDataBridge.I.GetPlayerSetup(character.GetComponent<CharacterController>().PlayerIndex).SelectedCharacter.CharacterName;
            AudioManager.I?.PlayCharacterSE(charaName, seType);
        }
    }
}
