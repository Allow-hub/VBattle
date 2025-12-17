using Cysharp.Threading.Tasks;
using UnityEngine;
using TechC.VBattle.Core.Util;

namespace TechC.VBattle.InGame.Camera
{
    /// <summary>
    /// カメラ追従・ズームエフェクトの実装
    /// 格闘ゲーム用のプレイヤー追従機能とズームエフェクトを統合
    /// </summary>
    [System.Serializable]
    public class CameraFollow : ICameraEffect
    {
        [Header("ズームエフェクト設定")]
        [SerializeField] private float zoomIntensity = 10f;
        [SerializeField] private float zoomDuration = 0.5f;
        
        [Header("格闘ゲーム追従設定")]
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float zoomSpeed = 3f;
        [SerializeField] private Vector2 marginSize = new Vector2(4f, 3f);
        [SerializeField] private float minFOV = 30f;
        [SerializeField] private float maxFOV = 60f;
        [SerializeField] private float jumpHeightOffset = 2f;
        [SerializeField] private float baseHeight = 0f;
        
        [Header("追従制限")]
        [SerializeField] private Vector2 followLimits = new Vector2(15f, 10f);
        
        private UnityEngine.Camera targetCamera;
        private Transform cameraTransform;
        private float originalFOV;
        private bool isZooming;
        private float currentIntensity;
        private float currentDuration;
        private float zoomStartTime;
        
        // 追従機能用
        private Transform player1;
        private Transform player2;
        private bool isFollowActive = false;
        private Vector3 targetPosition;
        private float targetFOV;

        public CameraEffectState State { get; private set; } = CameraEffectState.Idle;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="cameraTransform">対象のカメラTransform</param>
        public void Initialize(Transform cameraTransform)
        {
            this.cameraTransform = cameraTransform;
            targetCamera = cameraTransform.GetComponent<UnityEngine.Camera>();
            if (targetCamera != null)
            {
                originalFOV = targetCamera.fieldOfView;
                targetFOV = originalFOV;
            }
        }

        /// <summary>
        /// デフォルト設定でズームエフェクトを実行
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
        /// 格闘ゲーム用の追従モードを開始
        /// </summary>
        /// <param name="p1">プレイヤー1のTransform</param>
        /// <param name="p2">プレイヤー2のTransform</param>
        public void StartFollowMode(Transform p1, Transform p2)
        {
            player1 = p1;
            player2 = p2;
            isFollowActive = true;
            
            if (cameraTransform != null)
                targetPosition = cameraTransform.position;
        }

        /// <summary>
        /// 追従モードを停止
        /// </summary>
        public void StopFollowMode()
        {
            isFollowActive = false;
        }

        /// <summary>
        /// 毎フレーム呼び出してカメラ位置とFOVを調整
        /// </summary>
        public void UpdateFollow()
        {
            if (!isFollowActive || player1 == null || player2 == null || targetCamera == null)
                return;

            // プレイヤー中央位置計算
            Vector3 centerPosition = (player1.position + player2.position) * 0.5f;
            
            // Y軸にジャンプオフセットを追加（最も高いプレイヤーに合わせる）
            float highestY = Mathf.Max(player1.position.y, player2.position.y);
            float targetY = Mathf.Max(baseHeight, highestY + jumpHeightOffset);
            centerPosition.y = targetY;
            
            // カメラのZ位置は維持
            var controller = cameraTransform.GetComponent<CameraController>();
            if (controller != null)
                centerPosition.z = controller.OriginalPosition.z;
            
            // 追従制限を適用
            centerPosition.x = Mathf.Clamp(centerPosition.x, -followLimits.x, followLimits.x);
            centerPosition.y = Mathf.Clamp(centerPosition.y, -followLimits.y, followLimits.y);
            
            targetPosition = centerPosition;
            
            // カメラ位置をスムーズに移動
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, followSpeed * Time.deltaTime);
            
            // プレイヤー間距離に応じてFOV調整
            float distance = Vector3.Distance(player1.position, player2.position);
            float normalizedDistance = Mathf.Clamp01(distance / marginSize.x);
            targetFOV = Mathf.Lerp(minFOV, maxFOV, normalizedDistance);
            
            // FOVをスムーズに調整
            targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
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
                Reset(Vector3.zero);
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
            float targetFOVEffect = originalFOV - (currentIntensity * curve);

            targetCamera.fieldOfView = targetFOVEffect;
        }
    }
}
