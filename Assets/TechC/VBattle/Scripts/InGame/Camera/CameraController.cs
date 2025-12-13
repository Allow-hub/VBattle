using UnityEngine;

namespace TechC.VBattle.InGame.Camera
{
    /// <summary>
    /// カメラエフェクトの統合管理クラス
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("エフェクト設定")]
        [SerializeField] private CameraShake CameraShake = new CameraShake();

        private UnityEngine.Camera targetCamera;
        private Vector3 originalPosition;
        
        private float cachedShakeIntensity;
        private float cachedShakeDuration;

        private void Start()
        {
            targetCamera = GetComponent<UnityEngine.Camera>();

            originalPosition = transform.position;

            CameraShake.Initialize(transform);
            
            // デフォルト値をキャッシュ
            cachedShakeIntensity = CameraShake.DefaultIntensity;
            cachedShakeDuration = CameraShake.DefaultDuration;
        }

        private void Update()
        {
            // テスト用：スペースキーでシェイク
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                CameraShake.Apply(cachedShakeIntensity, cachedShakeDuration);
            }
        }
    }
}