using UnityEngine;
using System.Collections.Generic;

namespace TechC.VBattle.InGame.Npc
{
    /// <summary>
    /// AI行動の時間設定
    /// </summary>
    [System.Serializable]
    public class AIActionTimings
    {
        [Tooltip("行動間隔（秒）")]
        [SerializeField] private float actionInterval = 0.5f;
        
        [Tooltip("反応時間（秒）")]
        [SerializeField] private float reactionTime = 0.1f;
        
        [Tooltip("接近行動の継続時間（秒）")]
        [SerializeField] private float approachTime = 0.3f;
        
        [Tooltip("後退行動の継続時間（秒）")]
        [SerializeField] private float retreatTime = 0.3f;
        
        [Tooltip("弱攻撃の入力継続時間（秒）")]
        [SerializeField] private float weakAttackTime = 0.15f;
        
        [Tooltip("強攻撃の入力継続時間（秒）")]
        [SerializeField] private float strongAttackTime = 0.3f;
        
        [Tooltip("ガードの継続時間（秒）")]
        [SerializeField] private float guardTime = 0.3f;
        
        [Tooltip("ジャンプの入力継続時間（秒）")]
        [SerializeField] private float jumpTime = 0.12f;
        
        [Tooltip("しゃがみの継続時間（秒）")]
        [SerializeField] private float crouchTime = 0.25f;
        
        [Tooltip("待機行動の継続時間（秒）")]
        [SerializeField] private float waitTime = 0.25f;
        
        [Tooltip("ジャンプ/しゃがみ攻撃のタイミング調整率（0.0-1.0）")]
        [SerializeField] private float attackDelayRate = 0.5f;

        public float ActionInterval => actionInterval;
        public float ReactionTime => reactionTime;
        public float ApproachTime => approachTime;
        public float RetreatTime => retreatTime;
        public float WeakAttackTime => weakAttackTime;
        public float StrongAttackTime => strongAttackTime;
        public float GuardTime => guardTime;
        public float JumpTime => jumpTime;
        public float CrouchTime => crouchTime;
        public float WaitTime => waitTime;
        public float AttackDelayRate => attackDelayRate;
    }

    /// <summary>
    /// 攻撃方向の確率設定
    /// </summary>
    [System.Serializable]
    public class AIAttackDirectionProbability
    {
        [Header("通常時の確率")]
        [Tooltip("左方向への攻撃確率（通常時）")]
        [SerializeField] private float baseLeftPercent = 25f;
        
        [Tooltip("右方向への攻撃確率（通常時）")]
        [SerializeField] private float baseRightPercent = 25f;
        
        [Tooltip("上方向への攻撃確率（通常時）")]
        [SerializeField] private float baseUpPercent = 25f;
        
        [Tooltip("下方向への攻撃確率（通常時）")]
        [SerializeField] private float baseDownPercent = 25f;

        [Header("優遇時の確率")]
        [Tooltip("左優遇時の左方向確率")]
        [SerializeField] private float preferLeftPercent = 40f;
        
        [Tooltip("右優遇時の右方向確率")]
        [SerializeField] private float preferRightPercent = 40f;
        
        [Tooltip("左優遇時の右方向確率")]
        [SerializeField] private float lessRightPercent = 10f;
        
        [Tooltip("右優遇時の左方向確率")]
        [SerializeField] private float lessLeftPercent = 10f;

        public float BaseLeftPercent => baseLeftPercent;
        public float BaseRightPercent => baseRightPercent;
        public float BaseUpPercent => baseUpPercent;
        public float BaseDownPercent => baseDownPercent;
        public float PreferLeftPercent => preferLeftPercent;
        public float PreferRightPercent => preferRightPercent;
        public float LessRightPercent => lessRightPercent;
        public float LessLeftPercent => lessLeftPercent;
    }

    /// <summary>
    /// 攻撃設定
    /// </summary>
    [System.Serializable]
    public class AIAttackSettings
    {
        [Header("通常攻撃")]
        [Tooltip("弱攻撃を選択する確率")]
        [Range(0, 1)]
        [SerializeField] private float weakAttackChance = 0.7f;

        [Header("ジャンプ攻撃")]
        [Tooltip("ジャンプ中に攻撃する確率")]
        [Range(0, 1)]
        [SerializeField] private float jumpAttackChance = 0.8f;
        
        [Tooltip("ジャンプ攻撃時に弱攻撃を選択する確率")]
        [Range(0, 1)]
        [SerializeField] private float jumpWeakAttackChance = 0.7f;

        [Header("しゃがみ攻撃")]
        [Tooltip("しゃがみ中に攻撃する確率")]
        [Range(0, 1)]
        [SerializeField] private float crouchAttackChance = 0.6f;
        
        [Tooltip("しゃがみ攻撃時に弱攻撃を選択する確率")]
        [Range(0, 1)]
        [SerializeField] private float crouchWeakAttackChance = 0.7f;

        public float WeakAttackChance => weakAttackChance;
        public float JumpAttackChance => jumpAttackChance;
        public float JumpWeakAttackChance => jumpWeakAttackChance;
        public float CrouchAttackChance => crouchAttackChance;
        public float CrouchWeakAttackChance => crouchWeakAttackChance;
    }

    /// <summary>
    /// AI性格パラメータ
    /// </summary>
    [System.Serializable]
    public class AIPersonality
    {
        [Tooltip("攻撃性（高いほど攻撃的）")]
        [SerializeField] private float aggression = 1.0f;
        
        [Tooltip("防御性（高いほど防御的）")]
        [SerializeField] private float defensiveness = 1.0f;
        
        [Tooltip("機動性（高いほど移動・ジャンプを多用）")]
        [SerializeField] private float mobility = 1.0f;

        public float Aggression => aggression;
        public float Defensiveness => defensiveness;
        public float Mobility => mobility;
    }

    /// <summary>
    /// AI距離設定
    /// </summary>
    [System.Serializable]
    public class AIDistanceSettings
    {
        [Tooltip("近距離の判定閾値（メートル）")]
        [SerializeField] private float closeRange = 2.0f;
        
        [Tooltip("中距離の判定閾値（メートル）")]
        [SerializeField] private float mediumRange = 5.0f;

        public float CloseRange => closeRange;
        public float MediumRange => mediumRange;
    }

    /// <summary>
    /// NPC AIの設定データ（ScriptableObject）
    /// 難易度やAI性格ごとにプリセットを作成して使用する
    /// </summary>
    [CreateAssetMenu(fileName = "_NpcData_", menuName = "TechC/NPC Data", order = 0)]
    public class NpcDataSO : ScriptableObject
    {
        [Header("行動タイミング")]
        [SerializeField] private AIActionTimings actionTimings = new AIActionTimings();

        [Header("攻撃設定")]
        [SerializeField] private AIAttackSettings attackSettings = new AIAttackSettings();

        [Header("攻撃方向の確率")]
        [SerializeField] private AIAttackDirectionProbability directionProbability = new AIAttackDirectionProbability();

        [Header("AI性格")]
        [SerializeField] private AIPersonality personality = new AIPersonality();

        [Header("距離設定")]
        [SerializeField] private AIDistanceSettings distanceSettings = new AIDistanceSettings();

        [Header("戦略設定")]
        [Tooltip("各距離での行動戦略")]
        [SerializeField] private List<BattleRangeStrategy> strategies = new List<BattleRangeStrategy>();

        public AIActionTimings ActionTimings => actionTimings;
        public AIAttackSettings AttackSettings => attackSettings;
        public AIAttackDirectionProbability DirectionProbability => directionProbability;
        public AIPersonality Personality => personality;
        public AIDistanceSettings DistanceSettings => distanceSettings;
        public List<BattleRangeStrategy> Strategies => strategies;
    }
}
