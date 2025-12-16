using Cysharp.Threading.Tasks;
using UnityEngine;
using TechC.VBattle.Core.Util;

namespace TechC.VBattle.InGame.Camera
{
    /// <summary>
    /// カメラパンエフェクトの実装
    /// </summary>
    [System.Serializable]
    public class CameraPan : ICameraEffect
    {
        [Header("パン設定")]
        [SerializeField] private float panIntensity = 2.0f;
        [SerializeField] private float panDuration = 1.0f;
        [SerializeField] private AnimationCurve panCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private Transform cameraTransform;
        private bool isPanning;
        private float currentIntensity;
        private float currentDuration;
        private float panStartTime;
        private Vector3 targetOffset;

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
        /// デフォルト設定でパンを実行
        /// </summary>
        public void Apply()
        {
            if (State == CameraEffectState.Active)
            {
                var controller = cameraTransform?.GetComponent<CameraController>();
                if (controller != null)
                    Stop(controller.OriginalPosition);
            }

            currentIntensity = panIntensity;
            currentDuration = panDuration;

            // ランダムな方向にパン
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            targetOffset = new Vector3(randomDirection.x, randomDirection.y, 0f) * currentIntensity;

            State = CameraEffectState.Active;
            isPanning = true;

            StartPanAsync();
        }

        public void Stop(Vector3 originalPosition)
        {
            isPanning = false;
            panStartTime = 0f;
            Reset(originalPosition);
        }

        public void Reset(Vector3 originalPosition)
        {
            if (cameraTransform != null)
                cameraTransform.position = originalPosition;

            panStartTime = 0f;
            State = CameraEffectState.Completed;
        }

        private async void StartPanAsync()
        {
            await DelayUtility.StartRepeatedActionAsync(
                currentDuration,
                Time.fixedDeltaTime,
                () => { PerformPanStep(); return UniTask.CompletedTask; }
            );

            if (isPanning)
            {
                isPanning = false;
                var controller = cameraTransform?.GetComponent<CameraController>();
                if (controller != null)
                    Reset(controller.OriginalPosition);
            }
        }

        private void PerformPanStep()
        {
            if (!isPanning) return;

            // 開始時間を記録
            if (panStartTime == 0f)
                panStartTime = Time.time;

            float elapsedTime = Time.time - panStartTime;
            float progress = elapsedTime / currentDuration;

            // カーブを使用してスムーズなパン
            float curveValue = panCurve.Evaluate(progress);
            Vector3 currentOffset = targetOffset * curveValue;

            var controller = cameraTransform?.GetComponent<CameraController>();
            if (controller != null)
                cameraTransform.position = controller.OriginalPosition + currentOffset;
        }
    }
}
