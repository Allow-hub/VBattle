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
        [SerializeField] private CameraFollow cameraFollow = new CameraFollow();
        [SerializeField] private CameraZoom cameraZoom = new CameraZoom();

        private UnityEngine.Camera targetCamera;
        private Vector3 originalPosition;
        private BattleEventBus eventBus;
        
        // プレイヤー参照
        private Character.CharacterController player1;
        private Character.CharacterController player2;

        /// <summary>
        /// カメラの元の位置（読み取り専用）
        /// </summary>
        public Vector3 OriginalPosition => originalPosition;
        
        /// <summary>
        /// 現在のカメラ位置
        /// </summary>
        public Vector3 CurrentPosition => transform.position;

        private void Start()
        {
            targetCamera = GetComponent<UnityEngine.Camera>();
            originalPosition = transform.position;

            cameraShake.Initialize(transform);
            cameraFollow.Initialize(transform);
            cameraZoom.Initialize(transform);

            InitializeEventBus();
        }
        
        /// <summary>
        /// プレイヤー参照を設定
        /// </summary>
        /// <param name="p1">プレイヤー1</param>
        /// <param name="p2">プレイヤー2</param>
        public void SetupPlayers(Character.CharacterController p1, Character.CharacterController p2)
        {
            player1 = p1;
            player2 = p2;
            
            // プレイヤーが設定されたら自動で追従モード開始
            if (player1 != null && player2 != null)
                cameraFollow.StartFollowMode(player1.transform, player2.transform);
        }

        /// <summary>
        /// InGameManagerからBattleEventBusを取得してイベントを購読
        /// </summary>
        private void InitializeEventBus()
        {
            var inGameManager = InGameManager.I;
            if (inGameManager?.BattleBus == null)
            {
                CustomLogger.Error("InGameManager または BattleEventBus が見つかりません！カメラエフェクトは攻撃イベントに反応しません。");
                return;
            }

            eventBus = inGameManager.BattleBus;
            eventBus.Subscribe<AttackResultEvent>(OnAttackResult);
        }


        private void Update()
        {
            cameraFollow.Apply();
        }

        /// <summary>
        /// 攻撃結果イベントのハンドラー
        /// </summary>
        /// <param name="attackResult">攻撃結果データ</param>
        private void OnAttackResult(AttackResultEvent attackResult)
        {
            if (!attackResult.isHit) return;
            
            cameraShake.Apply();
            cameraZoom.Apply();
        }
        private void OnDestroy()
        {
            // イベント購読を解除
            eventBus?.Unsubscribe<AttackResultEvent>(OnAttackResult);
        }
    }
}