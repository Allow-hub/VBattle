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
        private float elapsedShakeTime;  // 経過時間を自分で管理
        private Vector3 shakeBasePosition;

        public CameraEffectState State { get; private set; } = CameraEffectState.Idle;

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
            if (State == CameraEffectState.Active)
                Stop(cameraTransform.position);

            shakeBasePosition = cameraTransform.position;
            elapsedShakeTime = 0f;

            State = CameraEffectState.Active;

            StartShakeAsync().Forget();
        }

        public void Stop(Vector3 originalPosition)
        {
            State = CameraEffectState.Idle;
            elapsedShakeTime = 0f;
            Reset(originalPosition);
        }

        public void Reset(Vector3 originalPosition)
        {
            if (cameraTransform != null)
                cameraTransform.position = originalPosition;

            elapsedShakeTime = 0f;
            State = CameraEffectState.Completed;
        }

        private async UniTaskVoid StartShakeAsync()
        {
            await DelayUtility.StartRepeatedActionWithPauseAsync(
                shakeDuration,
                Time.fixedDeltaTime,
                async () => await PerformShakeStep(),
                InGameManager.I.GetPauseStateFunc
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

            // ポーズ中は進まないように経過時間を自分で加算
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