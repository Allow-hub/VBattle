using System.Collections.Generic;
using UnityEngine;
using TechC.VBattle.Core.Util;

namespace TechC.VBattle.InGame.Npc
{
    /// <summary>
    /// 重み付けリストのユーティリティクラス
    /// 重み付け計算や正規化などの便利機能を提供
    /// </summary>
    public static class AIWeightUtility
    {
        /// <summary>
        /// 重み付けリストの総重量を計算
        /// </summary>
        /// <param name="weights">重み付けリスト</param>
        /// <returns>総重量</returns>
        public static float GetTotalWeight(List<AIActionWeight> weights)
        {
            float total = 0f;
            foreach (var weight in weights)
            {
                total += weight.weight;
            }
            return total;
        }

        /// <summary>
        /// 重み付けリストを正規化（合計を1.0にする）
        /// </summary>
        /// <param name="weights">重み付けリスト</param>
        public static void NormalizeWeights(List<AIActionWeight> weights)
        {
            float total = GetTotalWeight(weights);

            if (total <= 0f)
            {
                float equalWeight = 1f / weights.Count;
                foreach (var weight in weights)
                {
                    weight.weight = equalWeight;
                }
            }
            else
            {
                foreach (var weight in weights)
                {
                    weight.weight /= total;
                }
            }
        }

        /// <summary>
        /// 重み付けに基づいてランダムに行動を選択
        /// </summary>
        /// <param name="weights">重み付けリスト</param>
        /// <returns>選択された行動タイプ</returns>
        public static AIActionType SelectWeightedAction(List<AIActionWeight> weights)
        {
            if (weights == null || weights.Count == 0)
                return AIActionType.Wait;

            float totalWeight = GetTotalWeight(weights);
            if (totalWeight <= 0f)
                return weights[0].actionType;

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var weight in weights)
            {
                currentWeight += weight.weight;
                if (randomValue <= currentWeight)
                    return weight.actionType;
            }

            return weights[weights.Count - 1].actionType;
        }
    }
}
