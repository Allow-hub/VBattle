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
        public List<AIActionWeight> actionWeights = new List<AIActionWeight>();

        [Header("戦略調整")]
        [Tooltip("攻撃的な性格の強さ（0.0-2.0）\n1.0が標準、大きいほど攻撃的")]
        [Range(0f, 2f)]
        public float aggressiveness = 1f;

        [Tooltip("防御的な性格の強さ（0.0-2.0）\n1.0が標準、大きいほど防御的")]
        [Range(0f, 2f)]
        public float defensiveness = 1f;

        [Tooltip("機動性の高さ（0.0-2.0）\n1.0が標準、大きいほど移動・ジャンプ重視")]
        [Range(0f, 2f)]
        public float mobility = 1f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="range">戦闘距離</param>
        public BattleRangeStrategy(BattleRange range)
        {
            this.range = range;
            aggressiveness = 1f;
            defensiveness = 1f;
            mobility = 1f;
            InitializeDefaultWeights();
        }

        /// <summary>
        /// デフォルトコンストラクタ（Inspector用）
        /// </summary>
        public BattleRangeStrategy()
        {
        }

        /// <summary>
        /// 距離に応じたデフォルト重み付けを初期化
        /// </summary>
        public void InitializeDefaultWeights()
        {
            actionWeights.Clear();

            switch (range)
            {
                case BattleRange.Close:
                    InitializeCloseRangeWeights();
                    break;

                case BattleRange.Medium:
                    InitializeMediumRangeWeights();
                    break;

                case BattleRange.Far:
                    InitializeFarRangeWeights();
                    break;
            }
        }

        /// <summary>
        /// 近距離戦略の重み付け初期化
        /// </summary>
        private void InitializeCloseRangeWeights()
        {
            actionWeights.Add(new AIActionWeight(AIActionType.Attack, 0.4f));
            actionWeights.Add(new AIActionWeight(AIActionType.Guard, 0.3f));
            actionWeights.Add(new AIActionWeight(AIActionType.Retreat, 0.2f));
            actionWeights.Add(new AIActionWeight(AIActionType.Jump, 0.1f));
        }

        /// <summary>
        /// 中距離戦略の重み付け初期化
        /// </summary>
        private void InitializeMediumRangeWeights()
        {
            actionWeights.Add(new AIActionWeight(AIActionType.Approach, 0.4f));
            actionWeights.Add(new AIActionWeight(AIActionType.Attack, 0.3f));
            actionWeights.Add(new AIActionWeight(AIActionType.Wait, 0.2f));
            actionWeights.Add(new AIActionWeight(AIActionType.Jump, 0.1f));
        }

        /// <summary>
        /// 遠距離戦略の重み付け初期化
        /// </summary>
        private void InitializeFarRangeWeights()
        {
            actionWeights.Add(new AIActionWeight(AIActionType.Approach, 0.6f));
            actionWeights.Add(new AIActionWeight(AIActionType.Wait, 0.2f));
            actionWeights.Add(new AIActionWeight(AIActionType.Jump, 0.2f));
        }

        /// <summary>
        /// 性格パラメータに基づいて重み付けを調整
        /// この機能により、同じ距離でも異なる性格のAIを作成可能
        /// </summary>
        public void ApplyPersonalityAdjustments()
        {
            foreach (var weight in actionWeights)
            {
                float originalWeight = weight.weight;

                switch (weight.actionType)
                {
                    case AIActionType.Attack:
                        weight.SetWeight(originalWeight * aggressiveness);
                        break;

                    case AIActionType.Guard:
                        weight.SetWeight(originalWeight * defensiveness);
                        break;

                    case AIActionType.Approach:
                    case AIActionType.Retreat:
                    case AIActionType.Jump:
                        weight.SetWeight(originalWeight * mobility);
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
