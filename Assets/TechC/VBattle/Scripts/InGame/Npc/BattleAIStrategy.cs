using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.InGame.Npc
{
    /// <summary>
    /// AI戦略管理クラス（個別性格パラメータ対応版）
    /// </summary>
    public class BattleAIStrategy : MonoBehaviour
    {
        [Header("戦略設定")]
        [SerializeField] private List<BattleRangeStrategy> strategies = new List<BattleRangeStrategy>();

        [Header("距離設定")]
        [SerializeField] private float closeRange = 2.0f;
        [SerializeField] private float mediumRange = 5.0f;

        private float aggressiveness = 1.0f;
        private float defensiveness = 1.0f;
        private float mobility = 1.0f;

        private void Awake()
        {
            InitializeStrategies();
        }

        private void InitializeStrategies()
        {
            if (strategies.Count == 0)
            {
                strategies.Add(new BattleRangeStrategy(BattleRange.Close));
                strategies.Add(new BattleRangeStrategy(BattleRange.Medium));
                strategies.Add(new BattleRangeStrategy(BattleRange.Far));
            }

            foreach (var strategy in strategies)
            {
                if (strategy.actionWeights.Count == 0)
                {
                    strategy.InitializeDefaultWeights();
                }
            }
        }

        /// <summary>
        /// 距離に基づいて戦闘範囲を判定
        /// </summary>
        public BattleRange GetBattleRange(float distance)
        {
            if (distance <= closeRange)
                return BattleRange.Close;
            else if (distance <= mediumRange)
                return BattleRange.Medium;
            else
                return BattleRange.Far;
        }

        /// <summary>
        /// 指定された範囲の戦略を取得
        /// </summary>
        public BattleRangeStrategy GetStrategy(BattleRange range) => strategies.Find(s => s.range == range);

        /// <summary>
        /// 重み付けに基づいてランダムに行動を選択
        /// 各戦略の個別性格パラメータを使用
        /// </summary>
        public AIActionType SelectAction(BattleRange range)
        {
            var strategy = GetStrategy(range);
            if (strategy == null || strategy.actionWeights.Count == 0)
                return AIActionType.Wait;

            strategy.ApplyPersonalityAdjustments();

            return strategy.SelectAction();
        }

        /// <summary>
        /// 性格パラメータを設定するメソッド
        /// </summary>
        public void SetPersonality(float aggressiveness, float defensiveness, float mobility)
        {
            this.aggressiveness = aggressiveness;
            this.defensiveness = defensiveness;
            this.mobility = mobility;
        }
    }
}
