using Cysharp.Threading.Tasks;
using UnityEngine;
using TechC.VBattle.Core.Util;

namespace TechC.VBattle.InGame.Camera
{
    /// <summary>
    /// カメラズームエフェクトの実装
    /// </summary>
    [System.Serializable]
    public class CameraZoom : ICameraEffect
    {
        [Header("ズーム設定")]
        [SerializeField] private float zoomIntensity = 10f;
        [SerializeField] private float zoomDuration = 0.5f;
        
        // 読み取り専用プロパティ
        public float DefaultIntensity => zoomIntensity;
        public float DefaultDuration => zoomDuration;
        
        private UnityEngine.Camera targetCamera;
        private float originalFOV;
        private bool isZooming;

        public CameraEffectState State { get; private set; } = CameraEffectState.Idle;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="cameraTransform">対象のカメラTransform</param>
        public void Initialize(Transform cameraTransform)
        {
            targetCamera = cameraTransform.GetComponent<UnityEngine.Camera>();
            if (targetCamera != null)
            {
                originalFOV = targetCamera.fieldOfView;
            }
        }

        public void Apply(float intensity, float duration)
        {
            // TODO: 実装予定
        }

        /// <summary>
        /// デフォルト設定でズームを実行
        /// </summary>
        public void ApplyDefault()
        {
            Apply(zoomIntensity, zoomDuration);
        }

        public void Stop()
        {
            // TODO: 実装予定
        }

        public void Reset()
        {
            if (targetCamera != null)
            {
                targetCamera.fieldOfView = originalFOV;
            }
            
            State = CameraEffectState.Completed;
        }
    }
}
