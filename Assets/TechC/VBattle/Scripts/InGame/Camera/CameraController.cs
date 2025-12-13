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
        [SerializeField] private CameraZoom CameraZoom = new CameraZoom();
        [SerializeField] private CameraPan CameraPan = new CameraPan();

        private UnityEngine.Camera targetCamera;
        private Vector3 originalPosition;

        private void Start()
        {
            targetCamera = GetComponent<UnityEngine.Camera>();

            originalPosition = transform.position;

            CameraShake.Initialize(transform);
            CameraZoom.Initialize(transform);
            CameraPan.Initialize(transform);
        }

        private void Update()
        {
            // テスト用：各エフェクトのテスト
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
                CameraShake.Apply();

            if (UnityEngine.Input.GetKeyDown(KeyCode.Z))
                CameraZoom.Apply();

            if (UnityEngine.Input.GetKeyDown(KeyCode.X))
                CameraPan.Apply();
        }
    }
}