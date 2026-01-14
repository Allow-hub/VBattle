using UnityEngine;

namespace StageLighting
{
    public class RotateStageLight : MonoBehaviour
    {
        [SerializeField]
        private float rotateSpeed = 30f; // 度/秒

        private void Update()
        {
            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
        }
    }
}