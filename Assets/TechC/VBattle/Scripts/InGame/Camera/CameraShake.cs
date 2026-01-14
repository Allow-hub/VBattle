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
        private float elapsedShakeTime;
        private Vector3 shakeBasePosition;
        private CameraEffectState state = CameraEffectState.Idle;

        public CameraEffectState State => state;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="cameraTransform">対象のカメラTransform</param>
        public void Init(Transform cameraTransform)
        {
            this.cameraTransform = cameraTransform;
        }

        /// <summary>
        /// シェイクを適用
        /// </summary>
        public void Apply()
        {
            if (state == CameraEffectState.Active)
                Stop(cameraTransform.position);

            shakeBasePosition = cameraTransform.position;
            elapsedShakeTime = 0f;

            state = CameraEffectState.Active;

            StartShakeAsync().Forget();
        }

        public void Stop(Vector3 originalPosition)
        {
            state = CameraEffectState.Idle;
            elapsedShakeTime = 0f;
            Reset(originalPosition);
        }

        public void Reset(Vector3 originalPosition)
        {
            if (cameraTransform != null)
                cameraTransform.position = originalPosition;

            elapsedShakeTime = 0f;
            state = CameraEffectState.Completed;
        }

        private async UniTaskVoid StartShakeAsync()
        {
            await DelayUtility.StartRepeatedActionWithPauseAsync(
                shakeDuration,
                Time.fixedDeltaTime,
                async () => await PerformShakeStep(),
                InGameManager.I.GetPauseStateFunc
            );

            if (state == CameraEffectState.Active)
            {
                state = CameraEffectState.Idle;
                Reset(shakeBasePosition);
            }
        }

        private async UniTask PerformShakeStep()
        {
            if (state != CameraEffectState.Active) return;

            elapsedShakeTime += Time.fixedDeltaTime;
            float progress = elapsedShakeTime / shakeDuration;

            float curveValue = shakeCurve.Evaluate(progress);
            float shakeAmount = shakeIntensity * curveValue;

            Vector3 randomOffset = Random.insideUnitSphere * shakeAmount;
            randomOffset.z = 0f;

            cameraTransform.position = shakeBasePosition + randomOffset;

            await UniTask.Yield();
        }
    }
}