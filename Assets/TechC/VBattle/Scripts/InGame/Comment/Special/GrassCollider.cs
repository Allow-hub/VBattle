using TechC.VBattle.Core.Util;
using TechC.VBattle.Systems;
using UnityEngine;

namespace TechC.VBattle.InGame.Comment
{
    public class GrassCollider : MonoBehaviour
    {
        // 定数定義
        private const float ROTATION_0_DEGREES = 0f;
        private const float ROTATION_90_DEGREES = 90f;
        private const float ROTATION_180_DEGREES = 180f;
        private const float ROTATION_NEGATIVE_90_DEGREES = -90f;
        
        [SerializeField] private GameObject grassChar;
        [SerializeField] private GameObject grassEffect;
        [SerializeField] Rigidbody rb;
        private bool isReturning = false;
        [SerializeField] private float returnDelay = 3f;
        
        // このオブジェクトを持っているキャラクターの参照
        private Character.CharacterController holderCharacter;

        private void OnEnable()
        {
            if (grassChar != null) grassChar.SetActive(true);
            if (grassEffect != null) grassEffect.SetActive(false);
            isReturning = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            holderCharacter = null;
        }

        /// <summary>
        /// 親が変更されたときに自動的に呼ばれる
        /// HoldAbility/ThrowAbilityに依存せず、GrassCollider自身が持ち主を追跡
        /// </summary>
        private void OnTransformParentChanged()
        {
            // 親が設定された場合、その親からCharacterControllerを探す
            if (transform.parent != null)
                holderCharacter = transform.root.GetComponent<Character.CharacterController>();
            else
                // 親が解除された（投げられた）場合
                holderCharacter = null;
        }

        public async void OnTriggerEnter(Collider other)
        {
            string layerName = LayerMask.LayerToName(other.gameObject.layer);
            if (layerName == "Ground" || layerName == "Wall")
            {
                // プレイヤーがこのアイテムを持っている場合、HoldItemを解除
                if (holderCharacter != null && holderCharacter.HoldItem == gameObject)
                {
                    holderCharacter.SetHoldItem(null);
                    holderCharacter = null;
                }

                // 草コメントが壁などにくっつくように親を切断
                transform.SetParent(null);

                Vector3 contactPoint = other.ClosestPoint(transform.position);
                Vector3 direction = (transform.position - contactPoint).normalized;
                Quaternion targetRotation = Quaternion.identity;
                if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
                {
                    if (direction.y > 0)
                        targetRotation = Quaternion.Euler(0, 0, ROTATION_0_DEGREES);
                    else
                        targetRotation = Quaternion.Euler(0, 0, ROTATION_180_DEGREES);
                }
                else
                {
                    if (direction.x > 0)
                        targetRotation = Quaternion.Euler(0, 0, ROTATION_NEGATIVE_90_DEGREES);
                    else
                        targetRotation = Quaternion.Euler(0, 0, ROTATION_90_DEGREES);
                }
                transform.position = contactPoint;
                transform.rotation = targetRotation;
                if (rb != null) rb.constraints = RigidbodyConstraints.FreezeAll;
                if (grassChar != null) grassChar.SetActive(false);
                if (grassEffect != null) grassEffect.SetActive(true);
                // AudioManager.I.PlaySE(SEID.Grass); // TODO: 音を入れ終わったらコメントアウトを外す

                if (!isReturning)
                {
                    isReturning = true;
                    await DelayUtility.StartDelayedActionAsync(returnDelay, () =>
                    {
                        EffectFactory.I.ReturnEffect(gameObject);
                        isReturning = false;
                    });
                }
            }
        }
    }
}
