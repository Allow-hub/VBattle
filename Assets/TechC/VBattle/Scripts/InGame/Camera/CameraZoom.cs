using Cysharp.Threading.Tasks;
using UnityEngine;
using TechC.VBattle.Core.Util;
using TechC.VBattle.InGame;

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
        private float elapsedZoomTime;  // 経過時間を自分で管理
        private CameraEffectState state = CameraEffectState.Idle;

        public CameraEffectState State => state;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="cameraTransform">対象のカメラTransform</param>
        public void Init(Transform cameraTransform)
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
            if (state == CameraEffectState.Active)
                Stop(Vector3.zero);

            currentIntensity = zoomIntensity;
            currentDuration = zoomDuration;
            elapsedZoomTime = 0f;  // 経過時間をリセット

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
                currentDuration,
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

            // ポーズ中は進まないように経過時間を自分で加算
            elapsedZoomTime += Time.fixedDeltaTime;
            float progress = elapsedZoomTime / currentDuration;

            // サイン曲線でスムーズなズームイン/アウト
            float curve = Mathf.Sin(progress * Mathf.PI);
            float targetFOVEffect = originalFOV - (currentIntensity * curve);

            targetCamera.fieldOfView = targetFOVEffect;
        }
    }
}
