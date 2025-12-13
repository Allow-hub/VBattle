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

        private void Start()
        {
            targetCamera = GetComponent<UnityEngine.Camera>();
            if (targetCamera == null)
            {
                return;
            }

            originalPosition = transform.position;

            // CameraShakeを初期化
            CameraShake.Initialize(transform);
        }

        private void Update()
        {
            // テスト用：スペースキーでシェイク
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                CameraShake.Apply(CameraShake.DefaultIntensity, CameraShake.DefaultDuration);
            }
        }
    }
}