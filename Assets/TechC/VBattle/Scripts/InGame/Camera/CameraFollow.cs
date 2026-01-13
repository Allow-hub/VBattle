using Cysharp.Threading.Tasks;
using UnityEngine;
using TechC.VBattle.Core.Util;
using TechC.VBattle.InGame;

namespace TechC.VBattle.InGame.Camera
{
    /// <summary>
    /// カメラ追従エフェクトの実装
    /// 格闘ゲーム用のプレイヤー追従機能を担当
    /// </summary>
    [System.Serializable]
    public class CameraFollow : ICameraEffect
    {
        private const float HALF = 0.5f;

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

        private Transform player1;
        private Transform player2;
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
            if (targetCamera == null) return;

            originalFOV = targetCamera.fieldOfView;
            targetFOV = originalFOV;
        }

        /// <summary>
        /// 追従エフェクトを開始し、毎フレーム更新を実行
        /// </summary>
        public void Apply()
        {
            if (State != CameraEffectState.Active) return;
            if (InGameManager.I != null && InGameManager.I.IsPaused) return;

            UpdateFollow();
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
            State = CameraEffectState.Active;

            if (cameraTransform != null)
                targetPosition = cameraTransform.position;
        }

        /// <summary>
        /// 毎フレーム呼び出してカメラ位置とFOVを調整
        /// </summary>
        public void UpdateFollow()
        {
            if (State != CameraEffectState.Active || player1 == null || player2 == null || targetCamera == null) return;

            Vector3 centerPosition = (player1.position + player2.position) * HALF;

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
            State = CameraEffectState.Idle;
            Reset(originalPosition);
        }

        public void Reset(Vector3 originalPosition)
        {
            if (targetCamera != null)
                targetCamera.fieldOfView = originalFOV;

            State = CameraEffectState.Completed;
        }
    }
}
