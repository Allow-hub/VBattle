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
        private float currentIntensity;
        private float currentDuration;
        private bool isShaking;
        private float shakeStartTime;
        private Vector3 shakeBasePosition;
        public CameraEffectState State { get; private set; } = CameraEffectState.Idle;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="cameraTransform">対象のカメラTransform</param>
        public void Initialize(Transform cameraTransform)
        {
            this.cameraTransform = cameraTransform;
        }

        /// <summary>
        /// シェイクを適用
        /// </summary>
        public void Apply()
        {
            if (State == CameraEffectState.Active)
                Stop(cameraTransform.position);

            currentIntensity = shakeIntensity;
            currentDuration = shakeDuration;

            // シェイク開始時の現在位置を基準位置として保存
            shakeBasePosition = cameraTransform.position;

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
                // シェイク終了時は基準位置に戻す（追従は別途継続される）
                Reset(shakeBasePosition);
            }
        }

        public void Stop(Vector3 originalPosition)
        {
            isShaking = false;
            shakeStartTime = 0f;
            Reset(originalPosition);
        }

        public void Reset(Vector3 originalPosition)
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

            // シェイク開始時の基準位置を使用
            cameraTransform.position = shakeBasePosition + randomOffset;
        }
    }
}
