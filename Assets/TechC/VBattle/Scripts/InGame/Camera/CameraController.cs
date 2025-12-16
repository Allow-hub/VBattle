using UnityEngine;
using TechC.VBattle.InGame.Events;
using TechC.VBattle.InGame.Systems;
using TechC.VBattle.Core.Extensions;

namespace TechC.VBattle.InGame.Camera
{
    /// <summary>
    /// カメラエフェクトの統合管理クラス
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("エフェクト設定")]
        [SerializeField] private CameraShake cameraShake = new CameraShake();
        [SerializeField] private CameraZoom cameraZoom = new CameraZoom();
        [SerializeField] private CameraPan cameraPan = new CameraPan();

        private UnityEngine.Camera targetCamera;
        private Vector3 originalPosition;
        private BattleEventBus eventBus;

        /// <summary>
        /// カメラの元の位置（読み取り専用）
        /// </summary>
        public Vector3 OriginalPosition => originalPosition;

        private void Start()
        {
            targetCamera = GetComponent<UnityEngine.Camera>();

            originalPosition = transform.position;

            cameraShake.Initialize(transform);
            cameraZoom.Initialize(transform);
            cameraPan.Initialize(transform);

            // InGameManagerからBattleEventBusを取得して初期化
            InitializeEventBus();
        }

        /// <summary>
        /// InGameManagerからBattleEventBusを取得してイベントを購読
        /// </summary>
        private void InitializeEventBus()
        {
            var inGameManager = InGameManager.I;
            if (inGameManager != null && inGameManager.BattleBus != null)
            {
                eventBus = inGameManager.BattleBus;
                eventBus.Subscribe<AttackResultEvent>(OnAttackResult);
            }
            else
                CustomLogger.Error("InGameManager または BattleEventBus が見つかりません！カメラエフェクトは攻撃イベントに反応しません。");
        }

        private void OnDestroy()
        {
            // イベント購読を解除
            if (eventBus != null)
                eventBus.Unsubscribe<AttackResultEvent>(OnAttackResult);
        }
        private void Update()
        {
            // テスト用：各エフェクトのテスト
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
                cameraShake.Apply();

            if (UnityEngine.Input.GetKeyDown(KeyCode.Z))
                cameraZoom.Apply();

            if (UnityEngine.Input.GetKeyDown(KeyCode.X))
                cameraPan.Apply();
        }

        /// <summary>
        /// 攻撃結果イベントのハンドラー
        /// </summary>
        /// <param name="attackResult">攻撃結果データ</param>
        private void OnAttackResult(AttackResultEvent attackResult)
        {
            // ヒットした場合のみカメラシェイクを実行
            if (attackResult.isHit)
                cameraShake.Apply();
        }
    }
}