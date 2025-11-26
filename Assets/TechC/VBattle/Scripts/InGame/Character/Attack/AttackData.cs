using Cinemachine;
using TechC.VBattle.Audio;
using TechC.VBattle.InGame.Events;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 攻撃データ
    /// </summary>
    [CreateAssetMenu(fileName = "AttackData", menuName = "TechC/Combat/Attack Data")]
    public class AttackData : ScriptableObject
    {
        // ===== 基本情報 =====
        [Header("基本情報")]
        public CharaName charaName;
        public string attackName;

        [Tooltip("エフェクト・当たり判定含む攻撃オブジェクト")]
        public GameObject attackPrefab;

        [Tooltip("攻撃オブジェクトのローカル位置オフセット")]
        public Vector3 prefabOffset;

        [Tooltip("攻撃オブジェクトの回転")]
        public Vector3 prefabRotation;

        // ===== アニメーション関連 =====
        [Header("アニメーション")]
        [Tooltip("アニメーション再生速度")]
        public float animationSpeed = 1f;

        [Tooltip("攻撃全体の持続時間（アニメーション尺）")]
        public float attackDuration;

        // ===== 攻撃判定・条件 =====
        [Header("攻撃判定")]
        [Tooltip("攻撃方式")]
        public HitDetectionMode hitDetectionMode = HitDetectionMode.OverlapSphere;

        [Tooltip("攻撃が可能なレイヤー")]
        public LayerMask targetLayers;

        [Tooltip("攻撃の半径")]
        public float radius;

        [Tooltip("キャラクターからの相対位置")]
        public Vector3 hitboxOffset;

        [Tooltip("当たり判定の発生タイミング")]
        public float hitTiming;
        public GameObject hitEffectPrefab;

        // ===== 攻撃性能 =====
        [Header("攻撃特性")]
        [Tooltip("ダメージ量")]
        public int damage;

        [Tooltip("ノックバックの強さ")]
        public float knockback;

        [Tooltip("この攻撃がカウンター攻撃かどうか")]
        public bool isCounter;

        [Header("硬直")]
        [Tooltip("攻撃後の硬直時間（空振り時）")]
        public float recoveryDuration = 0.25f;
        [Header("キャンセル")]
        [Tooltip("攻撃キャンセル可能になるタイミング（秒）")]
        public float cancelStartTime = 0.1f;
        [Tooltip("キャンセル可能終了タイミング（秒）")]
        public float cancelEndTime = 0.4f;
        [Header("被弾硬直")]
        [Tooltip("相手が被弾して行動不能になる時間")]
        public float hitStunDuration = 0.25f;
        // ===== 連携・派生 =====
        [Header("連携・派生攻撃")]
        [Tooltip("連携先の攻撃データ")]
        public AttackData nextChain;

        [Tooltip("連携可能か")]
        public bool canChain;

        [Tooltip("前回連携オブジェクト地点に出すかどうか")]
        public bool isChainPos;

        // ===== ヒット演出（ヒットストップ・ノックバック・Shake） =====
        [Header("ヒット演出")]
        [Tooltip("ヒットストップの持続時間")]
        public float hitStopDuration;

        [Tooltip("ヒットストップ中の時間スケール")]
        public float hitStopTimeScale;

        [Tooltip("吹っ飛ぶ方向を定義（デフォルトは前方）")]
        public Vector3 knockbackDirection = Vector3.forward;

        [Tooltip("カメラシェイクの強さ")]
        public float shakeIntensity = 0.1f;

        [Tooltip("カメラシェイクの持続時間")]
        public float shakeDuraion = 0.1f;

        [Tooltip("シェイク用ノイズプロファイル")]
        public NoiseSettings noiseSettings;

        // ===== 繰り返し攻撃設定 =====
        [Header("繰り返し攻撃設定")]
        [Tooltip("繰り返し攻撃が可能か")]
        public bool canRepeat;

        [Tooltip("繰り返し攻撃の間隔")]
        public float repeatInterval;

        [Tooltip("繰り返し攻撃の継続時間")]
        public float repeatDuration;

        // ===== 壁バウンド設定 =====
        [Header("壁バウンド設定")]
        [Tooltip("この攻撃が壁バウンドを発生させるかどうか")]
        public bool causesWallBounce;

        [Tooltip("バウンドの強さ（跳ね返る速度）")]
        public float wallBounceForce = 8f;

        [Tooltip("バウンド時の上方向補正")]
        public float wallBounceVerticalBoost = 2f;

        [Tooltip("バウンド可能な時間")]
        public float wallBounceTime = 0.5f;


        // ===== サウンド・エフェクト =====
        [Header("サウンド・ボイス・SE")]
        public CharacterSEType characterSEType;
        public CharacterVoiceType characterVoiceType;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // hitTimingはcancelStartTimeより前でなければならない
            if (hitTiming > cancelStartTime)
            {
                Debug.LogWarning($"[{name}] hitTiming ({hitTiming}) should be less than cancelStartTime ({cancelStartTime}). Auto-adjusting...");
                hitTiming = Mathf.Min(hitTiming, cancelStartTime);
            }

            // cancelStartTimeはcancelEndTimeより前でなければならない
            if (cancelStartTime > cancelEndTime)
            {
                Debug.LogWarning($"[{name}] cancelStartTime ({cancelStartTime}) should be less than cancelEndTime ({cancelEndTime}). Auto-adjusting...");
                cancelStartTime = Mathf.Min(cancelStartTime, cancelEndTime);
            }

            // cancelEndTimeはattackDurationより前でなければならない
            if (cancelEndTime > attackDuration)
            {
                Debug.LogWarning($"[{name}] cancelEndTime ({cancelEndTime}) should be less than attackDuration ({attackDuration}). Auto-adjusting...");
                cancelEndTime = Mathf.Min(cancelEndTime, attackDuration);
            }

            // 負の値チェック
            hitTiming = Mathf.Max(0, hitTiming);
            cancelStartTime = Mathf.Max(0, cancelStartTime);
            cancelEndTime = Mathf.Max(0, cancelEndTime);
            attackDuration = Mathf.Max(0, attackDuration);
            recoveryDuration = Mathf.Max(0, recoveryDuration);
        }
#endif
    }
}