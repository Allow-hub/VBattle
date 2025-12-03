using System.Collections.Generic;
using UnityEngine;
using TechC.CommentSystem;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// CharacterControllerのバフ管理機能を担当するパーシャルクラス
    /// </summary>
    public partial class CharacterController
    {
        // ===== バフ倍率管理 =====
        private Dictionary<BuffType, Dictionary<int, float>> buffMultipliers = new Dictionary<BuffType, Dictionary<int, float>>();

        /// <summary>
        /// バフの倍率を追加する
        /// </summary>
        /// <param name="buffType">バフの種類</param>
        /// <param name="buffId">バフのID</param>
        /// <param name="multiplier">倍率</param>
        public void AddMultiplier(BuffType buffType, int buffId, float multiplier)
        {
            if (!buffMultipliers.ContainsKey(buffType))
            {
                buffMultipliers[buffType] = new Dictionary<int, float>();
            }
            buffMultipliers[buffType][buffId] = multiplier;
        }

        /// <summary>
        /// バフの倍率を削除する
        /// </summary>
        /// <param name="buffType">バフの種類</param>
        /// <param name="buffId">バフのID</param>
        /// <param name="multiplier">倍率（使用しないが互換性のため）</param>
        public void RemoveMultiplier(BuffType buffType, int buffId, float multiplier)
        {
            if (buffMultipliers.ContainsKey(buffType))
            {
                buffMultipliers[buffType].Remove(buffId);
            }
        }

        /// <summary>
        /// 現在の攻撃力倍率を取得する
        /// </summary>
        /// <returns>攻撃力倍率</returns>
        public float GetCurrentAttackMultiplier()
        {
            return GetMultiplier(BuffType.Attack);
        }

        /// <summary>
        /// 現在の速度倍率を取得する
        /// </summary>
        /// <returns>速度倍率</returns>
        public float GetCurrentSpeedMultiplier()
        {
            return GetMultiplier(BuffType.Speed);
        }

        /// <summary>
        /// 指定されたバフタイプの現在の倍率を取得する
        /// </summary>
        /// <param name="buffType">バフの種類</param>
        /// <returns>合計倍率</returns>
        private float GetMultiplier(BuffType buffType)
        {
            if (!buffMultipliers.ContainsKey(buffType))
                return 1.0f;

            float totalMultiplier = 1.0f;
            foreach (var multiplier in buffMultipliers[buffType].Values)
            {
                totalMultiplier *= multiplier;
            }
            return totalMultiplier;
        }

        /// <summary>
        /// すべてのバフをクリアする
        /// </summary>
        public void ClearAllBuffs()
        {
            buffMultipliers.Clear();
        }
    }
}
