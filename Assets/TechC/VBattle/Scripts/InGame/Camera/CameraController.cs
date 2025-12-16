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
        [SerializeField] private CameraPan cameraPan = new CameraPan();
        [SerializeField] private CameraFollow cameraFollow = new CameraFollow();

        private UnityEngine.Camera targetCamera;
        private Vector3 originalPosition;
        private BattleEventBus eventBus;
        
        // プレイヤー参照（BattleJudgeと同じパターン）
        private Character.CharacterController player1;
        private Character.CharacterController player2;

        /// <summary>
        /// カメラの元の位置（読み取り専用）
        /// </summary>
        public Vector3 OriginalPosition => originalPosition;

        private void Start()
        {
            targetCamera = GetComponent<UnityEngine.Camera>();

            originalPosition = transform.position;

            cameraShake.Initialize(transform);
            cameraPan.Initialize(transform);
            cameraFollow.Initialize(transform);

            // InGameManagerからBattleEventBusを取得して初期化
            InitializeEventBus();
        }
        
        /// <summary>
        /// プレイヤー参照を設定（InGameManagerから呼び出し）
        /// BattleJudgeと同じパターン
        /// </summary>
        /// <param name="p1">プレイヤー1</param>
        /// <param name="p2">プレイヤー2</param>
        public void SetupPlayers(Character.CharacterController p1, Character.CharacterController p2)
        {
            player1 = p1;
            player2 = p2;
            
            // プレイヤーが設定されたら自動で追従モード開始
            if (player1 != null && player2 != null)
            {
                StartFollowMode();
            }
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
            cameraFollow.UpdateFollow();

            // テスト用：各エフェクトのテスト
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
                cameraShake.Apply();

            if (UnityEngine.Input.GetKeyDown(KeyCode.Z))
                // cameraZoom.Apply();
                cameraFollow.Apply();

            if (UnityEngine.Input.GetKeyDown(KeyCode.X))
                cameraPan.Apply();
                
            // 追従モードテスト
            if (UnityEngine.Input.GetKeyDown(KeyCode.F))
                StartFollowMode();
                
            if (UnityEngine.Input.GetKeyDown(KeyCode.G))
                StopFollowMode();
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
        
        /// <summary>
        /// 追従モードを開始
        /// </summary>
        private void StartFollowMode()
        {
            // 保持したプレイヤー参照を使用（FindObjectsOfTypeを使わずメモリ効率が良い）
            if (player1 != null && player2 != null)
            {
                cameraFollow.StartFollowMode(player1.transform, player2.transform);
                Debug.Log("カメラ追従モード開始");
            }
            else
            {
                Debug.LogWarning("プレイヤーが設定されていません");
            }
        }
        
        /// <summary>
        /// 追従モードを停止
        /// </summary>
        private void StopFollowMode()
        {
            cameraFollow.StopFollowMode();
            Debug.Log("カメラ追従モード停止");
        }
    }
}