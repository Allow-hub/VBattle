using Cysharp.Threading.Tasks;
using UnityEngine;
using TechC.VBattle.Core.Util;

namespace TechC.VBattle.InGame.Camera
{
    /// <summary>
    /// カメラズームの実装
    /// </summary>
    [System.Serializable]
    public class CameraZoom : ICameraEffect
    {
        [Header("ズーム設定")]
        [SerializeField] private float zoomIntensity = 10f;
        [SerializeField] private float zoomDuration = 0.5f;

        private UnityEngine.Camera targetCamera;
        private float originalFOV;
        private float elapsedZoomTime;
        private CameraEffectState state = CameraEffectState.Idle;

        public CameraEffectState State => state;

        public void Init(Transform cameraTransform)
        {
            targetCamera = cameraTransform.GetComponent<UnityEngine.Camera>();

            if (targetCamera != null)
                originalFOV = targetCamera.fieldOfView;
        }

        public void Apply()
        {
            if (state == CameraEffectState.Active)
                Stop(Vector3.zero);

            elapsedZoomTime = 0f;
            state = CameraEffectState.Active;

            StartZoomAsync().Forget();
        }

        public void Stop(Vector3 originalPosition)
        {
            state = CameraEffectState.Idle;
            elapsedZoomTime = 0f;
            Reset(originalPosition);
        }

        public void Reset(Vector3 originalPosition)
        {
            if (targetCamera != null)
                targetCamera.fieldOfView = originalFOV;

            elapsedZoomTime = 0f;
            state = CameraEffectState.Completed;
        }

        private async UniTaskVoid StartZoomAsync()
        {
            await DelayUtility.StartRepeatedActionWithPauseAsync(
                zoomDuration,
                Time.fixedDeltaTime,
                () => { PerformZoomStep(); return UniTask.CompletedTask; },
                InGameManager.I.GetPauseStateFunc
            );

            if (state == CameraEffectState.Active)
            {
                state = CameraEffectState.Idle;
                Reset(Vector3.zero);
            }
        }

        private void PerformZoomStep()
        {
            if (state != CameraEffectState.Active || targetCamera == null) return;

            elapsedZoomTime += Time.fixedDeltaTime;
            float progress = elapsedZoomTime / zoomDuration;

            float curve = Mathf.Sin(progress * Mathf.PI);
            float targetFOVEffect = originalFOV - (zoomIntensity * curve);

            targetCamera.fieldOfView = targetFOVEffect;
        }
    }
}
