using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Systems;
using UnityEngine;

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
            var characterController = collider.GetComponentInParent<TechC.VBattle.InGame.Character.CharacterController>();
            
            // CharacterControllerが見つからない場合
            if (characterController == null)
            {
                CustomLogger.Warning($"CharacterController not found on {collider.gameObject.name}");
                return;
            }
            
            // 既にアイテムを持っている場合
            if (characterController.HoldItem != null) return;

            // HandPosが設定されていない場合
            if (characterController.HandPos == null)
            {
                CustomLogger.Error($"HandPos is not assigned on {characterController.gameObject.name}");
                return;
            }

            // EffectFactoryが利用可能かチェック
            if (EffectFactory.I == null)
            {
                CustomLogger.Error("EffectFactory is not available");
                return;
            }

            // 診断情報を出力
            CustomLogger.Info($"Attempting to get effect object - prefab: {(gameObject != null ? gameObject.name : "NULL")}, EffectFactory.I: {(EffectFactory.I != null ? "OK" : "NULL")}");
            
            GameObject obj = EffectFactory.I.GetEffectObj(
                gameObject,
                characterController.HandPos.position,
                Quaternion.identity
            );
            
            // オブジェクトが取得できなかった場合
            if (obj == null)
            {
                CustomLogger.Error($"Failed to get effect object from EffectFactory - prefab: {(gameObject != null ? gameObject.name : "NULL")}, position: {characterController.HandPos.position}");
                return;
            }
            
            CustomLogger.Info($"Successfully got effect object: {obj.name}");

            characterController.SetHoldItem(obj);
            AttachToHand(obj, characterController.HandPos);
        }

        /// <summary>
        /// オブジェクトを手に装着する
        /// </summary>
        private void AttachToHand(GameObject obj, Transform handTransform)
        {
            if (obj == null)
            {
                CustomLogger.Error("AttachToHand: obj is null");
                return;
            }
            
            if (handTransform == null)
            {
                CustomLogger.Error("AttachToHand: handTransform is null");
                return;
            }
            
            if (obj.transform == null)
            {
                CustomLogger.Error("AttachToHand: obj.transform is null");
                return;
            }
            
            obj.transform.SetParent(handTransform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }
    }
}