using UnityEngine;

public class RotateStageLight : MonoBehaviour
{
    public float rotateSpeed = 30f; // 度/秒

    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }
}