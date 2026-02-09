using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.InGame.Npc
{
    /// <summary>
    /// 距離別AI戦略設定クラス
    ///
    /// 戦略設計の考え方：
    ///
    /// 【近距離戦略】
    /// - 攻撃のチャンスが多い
    /// - 相手の攻撃も受けやすい
    /// - 攻撃40%、ガード30%、後退20%、ジャンプ10%
    ///
    /// 【中距離戦略】
    /// - 間合いの駆け引きが重要
    /// - 接近か攻撃かの選択肢
    /// - 接近40%、攻撃30%、待機20%、ジャンプ10%
    ///
    /// 【遠距離戦略】
    /// - 基本的に接近が必要
    /// - 相手の動きを観察できる
    /// - 接近60%、待機20%、ジャンプ20%
    /// </summary>
    [System.Serializable]
    public class BattleRangeStrategy
    {
        [Header("戦略設定")]
        [Tooltip("この戦略が適用される戦闘距離")]
        public BattleRange range;

        [Header("行動重み付け")]
        [Tooltip("この距離での各行動の重み付け設定")]
        public List<AIActionWeight> actionWeights;



        /// <summary>
        /// 性格パラメータに基づいて重み付けを調整
        /// </summary>
        /// <param name="personality">NpcDataSOから渡される性格パラメータ</param>
        public void ApplyPersonalityAdjustments(AIPersonality personality)
        {
            foreach (var weight in actionWeights)
            {
                float originalWeight = weight.weight;

                switch (weight.actionType)
                {
                    case AIActionType.Attack:
                        weight.SetWeight(originalWeight * personality.Aggression);
                        break;

                    case AIActionType.Guard:
                        weight.SetWeight(originalWeight * personality.Defensiveness);
                        break;

                    case AIActionType.Approach:
                    case AIActionType.Retreat:
                    case AIActionType.Jump:
                        weight.SetWeight(originalWeight * personality.Mobility);
                        break;

                    case AIActionType.Wait:
                        break;
                }
            }

            AIWeightUtility.NormalizeWeights(actionWeights);
        }

        /// <summary>
        /// 重み付けに基づいて行動を選択
        /// </summary>
        public AIActionType SelectAction() => AIWeightUtility.SelectWeightedAction(actionWeights);
    }
}
