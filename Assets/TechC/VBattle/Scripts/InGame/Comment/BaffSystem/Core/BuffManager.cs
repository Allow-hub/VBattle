using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.InGame.Comment
{
    /// <summary>
    /// プレイヤーに適用されているバフを管理
    /// バフの追加、更新、解除を担当
    /// </summary>
    public class BuffManager : MonoBehaviour
    {
        /* バフを保持するリスト。現在アクティブなバフを管理 */
        private List<BuffBase> activeBuffs = new List<BuffBase>();

        void Update()
        {
            /* アクティブなバフリストを更新 */
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                BuffBase buff = activeBuffs[i];
                buff.UpdateBuff(Time.deltaTime, gameObject);

                /* バフの残り時間が終了したらそのバフを削除 */
                if (buff.remainingTime <= 0)
                    RemoveBuff(buff);
            }
        }

        /// <summary>
        /// バフを適用する
        /// すでに同じ種類のバフがアクティブな場合、そのバフの残り時間をリセットして再適用する
        /// </summary>
        /// <param name="buff">適用するバフ</param>
        public void ApplyBuff(BuffBase buff)
        {
            // 既にアクティブなバフをチェック
            foreach (BuffBase activeBuff in activeBuffs)
            {
                if (activeBuff.GetType() == buff.GetType())
                {
                    /* 同じバフが既に適用されている場合、バフの残り時間をリセット */
                    activeBuff.ResetDuration();
                    return;
                }
            }

            // 新しいバフをリストに追加
            activeBuffs.Add(buff);
            buff.ResetDuration();
            buff.Apply(gameObject);
        }

        /// <summary>
        /// 指定したバフを解除
        /// </summary>
        /// <param name="buff">解除するバフ</param>
        public void RemoveBuff(BuffBase buff)
        {
            // バフがアクティブリストに含まれている場合、解除
            if (activeBuffs.Contains(buff))
            {
                buff.Remove(gameObject);
                activeBuffs.Remove(buff);
            }
        }
    }
}
