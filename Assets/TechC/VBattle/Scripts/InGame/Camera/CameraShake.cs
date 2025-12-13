using Cysharp.Threading.Tasks;
using UnityEngine;
using TechC.VBattle.Core.Util;

namespace TechC.VBattle.InGame.Camera
{
    /// <summary>
    /// カメラシェイクエフェクトの実装
    /// </summary>
    [System.Serializable]
    public class CameraShake : ICameraEffect
    {
        [Header("シェイク設定")]
        [SerializeField] private float shakeIntensity = 0.5f;
        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                
        private Transform cameraTransform;
        private Vector3 originalPosition;
        private float currentIntensity;
        private float currentDuration;
        private bool isShaking;
        private float shakeStartTime;

        public CameraEffectState State { get; private set; } = CameraEffectState.Idle;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="cameraTransform">対象のカメラTransform</param>
        public void Initialize(Transform cameraTransform)
        {
            this.cameraTransform = cameraTransform;
            originalPosition = cameraTransform.position;
        }

        /// <summary>
        /// デフォルト設定でシェイクを適用
        /// </summary>
        public void Apply()
        {
            if (State == CameraEffectState.Active)
                Stop();

            currentIntensity = shakeIntensity;
            currentDuration = shakeDuration;
            
            State = CameraEffectState.Active;
            isShaking = true;
            
            StartShakeAsync();
        }


        private async void StartShakeAsync()
        {
            await DelayUtility.StartRepeatedActionAsync(
                currentDuration, 
                Time.fixedDeltaTime, // 60FPS相当の間隔
                async () => await PerformShakeStep()
            );
            
            if (isShaking)
            {
                isShaking = false;
                Reset();
            }
        }

        public void Stop()
        {
            isShaking = false;
            shakeStartTime = 0f;
            Reset();
        }

        public void Reset()
        {
            if (cameraTransform != null)
                cameraTransform.position = originalPosition;
            
            shakeStartTime = 0f;
            State = CameraEffectState.Completed;
        }

        private async UniTask PerformShakeStep()
        {
            if (!isShaking) return;

            // 開始時間を記録
            if (shakeStartTime == 0f)
                shakeStartTime = Time.time;

            float elapsedTime = Time.time - shakeStartTime;
            float progress = elapsedTime / currentDuration;

            float curveValue = shakeCurve.Evaluate(progress);
            float shakeAmount = currentIntensity * curveValue;

            Vector3 randomOffset = Random.insideUnitSphere * shakeAmount;
            randomOffset.z = 0f; // Z軸の振動は制限

            cameraTransform.position = originalPosition + randomOffset;
        }
    }
}
