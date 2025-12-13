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
        
        // 読み取り専用プロパティ
        public float DefaultIntensity => panIntensity;
        public float DefaultDuration => panDuration;
        
        private Transform cameraTransform;
        private Vector3 originalPosition;
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
            originalPosition = cameraTransform.position;
        }

        public void Apply(float intensity, float duration)
        {
            if (State == CameraEffectState.Active)
                Stop();

            currentIntensity = intensity > 0 ? intensity : panIntensity;
            currentDuration = duration > 0 ? duration : panDuration;
            
            // ランダムな方向にパン
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            targetOffset = new Vector3(randomDirection.x, randomDirection.y, 0f) * currentIntensity;
            
            State = CameraEffectState.Active;
            isPanning = true;
            
            StartPanAsync();
        }

        /// <summary>
        /// デフォルト設定でパンを実行
        /// </summary>
        public void ApplyDefault()
        {
            Apply(panIntensity, panDuration);
        }

        /// <summary>
        /// 指定方向にパン
        /// </summary>
        /// <param name="direction">パン方向（正規化済み）</param>
        /// <param name="intensity">パン強度</param>
        /// <param name="duration">継続時間</param>
        public void ApplyDirectional(Vector2 direction, float intensity, float duration)
        {
            if (State == CameraEffectState.Active)
                Stop();

            currentIntensity = intensity > 0 ? intensity : panIntensity;
            currentDuration = duration > 0 ? duration : panDuration;
            
            // 指定方向にパン
            Vector3 normalizedDirection = direction.normalized;
            targetOffset = new Vector3(normalizedDirection.x, normalizedDirection.y, 0f) * currentIntensity;
            
            State = CameraEffectState.Active;
            isPanning = true;
            
            StartPanAsync();
        }

        public void Stop()
        {
            isPanning = false;
            panStartTime = 0f;
            Reset();
        }

        public void Reset()
        {
            if (cameraTransform != null)
            {
                cameraTransform.position = originalPosition;
            }
            
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
                Reset();
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

            cameraTransform.position = originalPosition + currentOffset;
        }
    }
}
