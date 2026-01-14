using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    public class MovingHeadSweep : MonoBehaviour
    {
        [SerializeField]
        private float panAngle = 45f;

        [SerializeField]
        private float tiltAngle = 20f;

        [SerializeField]
        private float panSpeed = 1.2f;

        [SerializeField]
        private float tiltSpeed = 0.9f;

        [SerializeField]
        private float phaseOffset = 0f;

        private void Update()
        {
            float t = Time.time + phaseOffset;

            float pan  = Mathf.Sin(t * panSpeed)  * panAngle;
            float tilt = Mathf.Sin(t * tiltSpeed) * tiltAngle;

            transform.localRotation = Quaternion.Euler(tilt, pan, 0f);
        }
    }
}