using Cysharp.Threading.Tasks;
using UnityEngine;
using TechC.VBattle.Core.Util;

namespace TechC.VBattle.InGame.Camera
{
    /// <summary>
    /// カメラズームエフェクト専用クラス
    /// 攻撃時などの一時的なズーム演出を担当
    /// </summary>
    [System.Serializable]
    public class CameraZoom : ICameraEffect
    {
        [Header("ズームエフェクト設定")]
        [SerializeField] private float zoomIntensity = 10f;
        [SerializeField] private float zoomDuration = 0.5f;
        
        private UnityEngine.Camera targetCamera;
        private Transform cameraTransform;
        private float originalFOV;
        private float currentIntensity;
        private float currentDuration;
        private float zoomStartTime;

        public CameraEffectState State { get; private set; } = CameraEffectState.Idle;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="cameraTransform">対象のカメラTransform</param>
        public void Initialize(Transform cameraTransform)
        {
            this.cameraTransform = cameraTransform;
            targetCamera = cameraTransform.GetComponent<UnityEngine.Camera>();
            
            if (targetCamera != null)
                originalFOV = targetCamera.fieldOfView;
        }

        /// <summary>
        /// デフォルト設定でズームエフェクトを実行
        /// </summary>
        public void Apply()
        {
            if (State == CameraEffectState.Active)
                Stop(Vector3.zero);

            currentIntensity = zoomIntensity;
            currentDuration = zoomDuration;
            
            State = CameraEffectState.Active;
            
            StartZoomAsync().Forget();
        }

        public void Stop(Vector3 originalPosition)
        {
            State = CameraEffectState.Idle;
            zoomStartTime = 0f;
            Reset(originalPosition);
        }

        public void Reset(Vector3 originalPosition)
        {
            if (targetCamera != null)
                targetCamera.fieldOfView = originalFOV;
            
            zoomStartTime = 0f;
            State = CameraEffectState.Completed;
        }

        private async UniTaskVoid StartZoomAsync()
        {
            await DelayUtility.StartRepeatedActionAsync(
                currentDuration, 
                Time.fixedDeltaTime,
                () => { PerformZoomStep(); return UniTask.CompletedTask; }
            );
            
            if (State == CameraEffectState.Active)
            {
                State = CameraEffectState.Idle;
                Reset(Vector3.zero);
            }
        }

        private void PerformZoomStep()
        {
            if (State != CameraEffectState.Active || targetCamera == null) return;

            if (zoomStartTime == 0f)
                zoomStartTime = Time.time;

            float elapsedTime = Time.time - zoomStartTime;
            float progress = elapsedTime / currentDuration;

            // サイン曲線でスムーズなズームイン/アウト
            float curve = Mathf.Sin(progress * Mathf.PI);
            float targetFOVEffect = originalFOV - (currentIntensity * curve);

            targetCamera.fieldOfView = targetFOVEffect;
        }
    }
}
