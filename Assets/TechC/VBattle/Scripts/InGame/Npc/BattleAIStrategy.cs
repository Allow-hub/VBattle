using System.Collections.Generic;
using TechC.VBattle.Core.Extensions;

namespace TechC.VBattle.InGame.Npc
{
    /// <summary>
    /// AI戦略管理クラス（ロジックのみ）
    /// </summary>
    [System.Serializable]
    public class BattleAIStrategy
    {
        private List<BattleRangeStrategy> strategies;
        private float closeRange;
        private float mediumRange;
        private AIPersonality personality;

        /// <summary>
        /// 初期化（NpcDataSOから設定を受け取る）
        /// </summary>
        public void Initialize(NpcDataSO npcData)
        {
            if (npcData == null)
            {
                CustomLogger.Error("NpcDataSOがnullです");
                return;
            }

            // SOからデータをコピー
            closeRange = npcData.DistanceSettings.CloseRange;
            mediumRange = npcData.DistanceSettings.MediumRange;
            strategies = new List<BattleRangeStrategy>(npcData.Strategies);
            personality = npcData.Personality;
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
        public BattleRangeStrategy GetStrategy(BattleRange range) => strategies?.Find(s => s.range == range);

        /// <summary>
        /// 重み付けに基づいてランダムに行動を選択
        /// </summary>
        public AIActionType SelectAction(BattleRange range)
        {
            var strategy = GetStrategy(range);
            if (strategy == null || strategy.actionWeights.Count == 0)
                return AIActionType.Wait;

            strategy.ApplyPersonalityAdjustments(personality);

            return strategy.SelectAction();
        }
    }
}
