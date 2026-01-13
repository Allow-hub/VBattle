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

            shakeBasePosition = cameraTransform.position;

            State = CameraEffectState.Active;

            StartShakeAsync().Forget();
        }

        public void Stop(Vector3 originalPosition)
        {
            State = CameraEffectState.Idle;
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

        private async UniTaskVoid StartShakeAsync()
        {
            await DelayUtility.StartRepeatedActionAsync(
                shakeDuration,
                Time.fixedDeltaTime,
                async () => await PerformShakeStep()
            );

            if (State == CameraEffectState.Active)
            {
                State = CameraEffectState.Idle;
                Reset(shakeBasePosition);
            }
        }

        private async UniTask PerformShakeStep()
        {
            if (State != CameraEffectState.Active) return;

            if (shakeStartTime == 0f)
                shakeStartTime = Time.time;

            float elapsedTime = Time.time - shakeStartTime;
            float progress = elapsedTime / shakeDuration;

            float curveValue = shakeCurve.Evaluate(progress);
            float shakeAmount = shakeIntensity * curveValue;

            Vector3 randomOffset = Random.insideUnitSphere * shakeAmount;
            randomOffset.z = 0f;

            cameraTransform.position = shakeBasePosition + randomOffset;

            await UniTask.Yield();
        }
    }
}