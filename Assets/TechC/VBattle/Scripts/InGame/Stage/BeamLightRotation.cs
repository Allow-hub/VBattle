using UnityEngine;

public class MovingHeadSweep : MonoBehaviour
{
    public float panAngle = 45f;
    public float tiltAngle = 20f;
    public float panSpeed = 1.2f;
    public float tiltSpeed = 0.9f;

    public float phaseOffset = 0f;   

    void Update()
    {
        float t = Time.time + phaseOffset;

        float pan  = Mathf.Sin(t * panSpeed)  * panAngle;
        float tilt = Mathf.Sin(t * tiltSpeed) * tiltAngle;

        transform.localRotation = Quaternion.Euler(tilt, pan, 0f);
    }
}