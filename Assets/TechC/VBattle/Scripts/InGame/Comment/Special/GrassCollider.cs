using TechC.VBattle.Core.Util;
using TechC.VBattle.Systems;
using UnityEngine;

namespace TechC.VBattle.InGame.Comment
{
    public class GrassCollider : MonoBehaviour
    {
        // 定数定義
        private const float DEFAULT_RETURN_DELAY = 3f;
        private const float ROTATION_0_DEGREES = 0f;
        private const float ROTATION_90_DEGREES = 90f;
        private const float ROTATION_180_DEGREES = 180f;
        private const float ROTATION_NEGATIVE_90_DEGREES = -90f;
        
        [SerializeField] private GameObject grassChar;
        [SerializeField] private GameObject grassEffect;
        [SerializeField] Rigidbody rb;
        private bool isReturning = false;
        [SerializeField] private float returnDelay = DEFAULT_RETURN_DELAY;

        private void OnEnable()
        {
            if (grassChar != null) grassChar.SetActive(true);
            if (grassEffect != null) grassEffect.SetActive(false);
            isReturning = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        public async void OnTriggerEnter(Collider other)
        {
            string layerName = LayerMask.LayerToName(other.gameObject.layer);
            if (layerName == "Ground" || layerName == "Wall")
            {
                // TODO:Playerが草コメントを持った状態で壁に当ると草コメント保持しながら戦闘し、再度草コメントを拾えなくなる

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
