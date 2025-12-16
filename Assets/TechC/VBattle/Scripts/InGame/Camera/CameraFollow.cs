using UnityEngine;

namespace TechC.VBattle.InGame.Camera
{
    /// <summary>
    /// カメラ追従の実装
    /// 格闘ゲーム用のプレイヤー追従機能
    /// </summary>
    [System.Serializable]
    public class CameraFollow : ICameraEffect
    {
        [Header("格闘ゲーム追従設定")]
        [SerializeField] private bool enableFollowMode = false;
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float zoomSpeed = 3f;
        [SerializeField] private Vector2 marginSize = new Vector2(4f, 3f);
        [SerializeField] private float minFOV = 30f;
        [SerializeField] private float maxFOV = 60f;
        [SerializeField] private float jumpHeightOffset = 2f;
        [SerializeField] private float baseHeight = 0f;
        
        [Header("追従制限")]
        [SerializeField] private Vector2 followLimits = new Vector2(15f, 10f);
        
        // 定数定義
        private const float MIDPOINT_FACTOR = 0.5f; // プレイヤー間の中央位置計算用
        
        private UnityEngine.Camera targetCamera;
        private Transform cameraTransform;
        private float originalFOV;
        
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
        /// フォローモードを開始（ICameraEffectインターフェース実装）
        /// </summary>
        public void Apply()
        {
            // フォローモードがアクティブでない場合は何もしない
            if (!isFollowActive)
            {
                Debug.LogWarning("プレイヤーが設定されていないため、フォローできません");
            }
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
            enableFollowMode = true;
            
            if (cameraTransform != null)
            {
                targetPosition = cameraTransform.position;
            }
        }

        /// <summary>
        /// 追従モードを停止
        /// </summary>
        public void StopFollowMode()
        {
            isFollowActive = false;
            enableFollowMode = false;
        }

        /// <summary>
        /// 毎フレーム呼び出してカメラ位置とFOVを調整
        /// </summary>
        public void UpdateFollow()
        {
            if (!isFollowActive || !enableFollowMode || player1 == null || player2 == null || targetCamera == null)
                return;

            // プレイヤー中央位置計算
            Vector3 centerPosition = (player1.position + player2.position) * MIDPOINT_FACTOR;
            
            // Y軸にジャンプオフセットを追加（最も高いプレイヤーに合わせる）
            float highestY = Mathf.Max(player1.position.y, player2.position.y);
            float targetY = Mathf.Max(baseHeight, highestY + jumpHeightOffset);
            centerPosition.y = targetY;
            
            // カメラのZ位置は維持
            var controller = cameraTransform.GetComponent<CameraController>();
            if (controller != null)
            {
                centerPosition.z = controller.OriginalPosition.z;
            }
            
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
            StopFollowMode();
            Reset(originalPosition);
        }

        public void Reset(Vector3 originalPosition)
        {
            if (targetCamera != null)
            {
                targetCamera.fieldOfView = originalFOV;
            }
            
            State = CameraEffectState.Completed;
        }


    }
}
