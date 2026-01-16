using UnityEngine;
using TechC.VBattle.InGame.Events;

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

        private Vector3 originalPosition;

        /// <summary> カメラの元の位置（読み取り専用）</summary>        
        public Vector3 OriginalPosition => originalPosition;

        private void Start()
        {
            originalPosition = transform.position;

            cameraShake.Init(transform);
            cameraFollow.Init(transform);
            cameraZoom.Init(transform);

            InitializeEventBus();
        }

        /// <summary>
        /// プレイヤー参照を設定
        /// </summary>
        /// <param name="p1">プレイヤー1</param>
        /// <param name="p2">プレイヤー2</param>
        public void SetupPlayers(Character.CharacterController p1, Character.CharacterController p2)
        {
            if (p1 != null && p2 != null)
                cameraFollow.StartFollowMode(p1.transform, p2.transform);
        }

        /// <summary>
        /// InGameManagerからBattleEventBusを取得してイベントを購読
        /// </summary>
        private void InitializeEventBus()
        {
            var eventBus = InGameManager.I?.BattleBus;
            eventBus.Subscribe<AttackResultEvent>(OnAttackResult);
        }


        private void Update()
        {
            if (InGameManager.I.IsPaused) return;

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
            if (InGameManager.I != null && InGameManager.I.BattleBus != null)
                InGameManager.I.BattleBus.Unsubscribe<AttackResultEvent>(OnAttackResult);
        }
    }
}