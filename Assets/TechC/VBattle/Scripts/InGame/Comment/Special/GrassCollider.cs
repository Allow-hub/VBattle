using TechC.VBattle.Audio;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Util;
using TechC.VBattle.Systems;
using UnityEngine;

namespace TechC.CommentSystem
{
    public class GrassCollider : MonoBehaviour
    {
        [SerializeField] private GameObject grassChar;
        [SerializeField] private GameObject grassEffect;
        [SerializeField] Rigidbody rb;
        private bool isReturning = false;
        [SerializeField] private float returnDelay = 3f;

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
                Vector3 contactPoint = other.ClosestPoint(transform.position);
                Vector3 direction = (transform.position - contactPoint).normalized;
                Quaternion targetRotation = Quaternion.identity;
                if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
                {
                    if (direction.y > 0)
                        targetRotation = Quaternion.Euler(0, 0, 0f);
                    else
                        targetRotation = Quaternion.Euler(0, 0, 180f);
                }
                else
                {
                    if (direction.x > 0)
                        targetRotation = Quaternion.Euler(0, 0, -90f);
                    else
                        targetRotation = Quaternion.Euler(0, 0, 90f);
                }
                transform.position = contactPoint;
                transform.rotation = targetRotation;
                if (rb != null) rb.constraints = RigidbodyConstraints.FreezeAll;
                if (grassChar != null) grassChar.SetActive(false);
                if (grassEffect != null) grassEffect.SetActive(true);
                AudioManager.I.PlaySE(SEID.Grass);

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
