using UnityEngine;

namespace TechC.VBattle.InGame.Npc
{
    /// <summary>
    /// AI行動の重み付けデータ
    /// 1. 基本概念
    /// - 各行動に0.0〜1.0の重み（確率）を設定
    /// - 重みが大きいほど選択されやすい
    /// - 全ての重みの合計値で正規化される
    ///
    /// 2. 重み付けの例
    /// 近距離での重み設定：
    /// - Attack: 0.4 (40%)
    /// - Guard: 0.3 (30%)
    /// - Retreat: 0.2 (20%)
    /// - Jump: 0.1 (10%)
    /// 合計: 1.0 (100%)
    /// </summary>
    [System.Serializable]
    public class AIActionWeight
    {
        private const float DEFAULT_WEIGHT = 0.1f;

        [Header("行動設定")]
        [Tooltip("実行する行動の種類")]
        public AIActionType actionType;

        [Header("重み設定")]
        [Tooltip("この行動が選択される確率の重み（0.0-1.0）\n" +
                "値が大きいほど選択されやすくなります")]
        [Range(0f, 1f)]
        public float weight;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="type">行動タイプ</param>
        /// <param name="weight">重み（0.0-1.0）</param>
        public AIActionWeight(AIActionType type, float weight)
        {
            this.actionType = type;
            this.weight = Mathf.Clamp01(weight); // 0-1の範囲に制限
        }

        /// <summary>
        /// デフォルトコンストラクタ（Inspector用）
        /// </summary>
        public AIActionWeight()
        {
            this.actionType = AIActionType.Wait;
            this.weight = DEFAULT_WEIGHT;
        }

        /// <summary>重みを設定（範囲チェック付き）</summary>
        public void SetWeight(float newWeight) => weight = Mathf.Clamp01(newWeight);
    }
}
