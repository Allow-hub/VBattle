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
        
        // プロパティは削除（不要になった）
        
        private UnityEngine.Camera targetCamera;
        private float originalFOV;
        private bool isZooming;
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
            targetCamera = cameraTransform.GetComponent<UnityEngine.Camera>();
            if (targetCamera != null)
            {
                originalFOV = targetCamera.fieldOfView;
            }
        }

        /// <summary>
        /// デフォルト設定でズームを実行
        /// </summary>
        public void Apply()
        {
            if (State == CameraEffectState.Active)
                Stop(Vector3.zero);

            currentIntensity = zoomIntensity;
            currentDuration = zoomDuration;
            
            State = CameraEffectState.Active;
            isZooming = true;
            
            StartZoomAsync();
        }

        /// <summary>
        /// カスタム設定でズームを実行
        /// </summary>
        /// <param name="intensity">ズーム強度</param>
        /// <param name="duration">継続時間</param>
        public void ApplyCustom(float intensity, float duration)
        {
            if (State == CameraEffectState.Active)
                Stop(Vector3.zero); // ズームはFOVのみを制御するのでoriginalPositionは不要

            currentIntensity = intensity;
            currentDuration = duration;
            
            State = CameraEffectState.Active;
            isZooming = true;
            
            StartZoomAsync();
        }

        public void Stop(Vector3 originalPosition)
        {
            isZooming = false;
            zoomStartTime = 0f;
            Reset(originalPosition);
        }

        public void Reset(Vector3 originalPosition)
        {
            if (targetCamera != null)
            {
                targetCamera.fieldOfView = originalFOV;
            }
            
            zoomStartTime = 0f;
            State = CameraEffectState.Completed;
        }

        private async void StartZoomAsync()
        {
            await DelayUtility.StartRepeatedActionAsync(
                currentDuration, 
                Time.fixedDeltaTime,
                () => { PerformZoomStep(); return UniTask.CompletedTask; }
            );
            
            if (isZooming)
            {
                isZooming = false;
                Reset(Vector3.zero); // ズームはFOVのみを制御するのでoriginalPositionは不要
            }
        }

        private void PerformZoomStep()
        {
            if (!isZooming || targetCamera == null) return;

            // 開始時間を記録
            if (zoomStartTime == 0f)
                zoomStartTime = Time.time;

            float elapsedTime = Time.time - zoomStartTime;
            float progress = elapsedTime / currentDuration;

            // サイン曲線でスムーズなズームイン/アウト
            float curve = Mathf.Sin(progress * Mathf.PI);
            float targetFOV = originalFOV - (currentIntensity * curve);

            targetCamera.fieldOfView = targetFOV;
        }
    }
}
