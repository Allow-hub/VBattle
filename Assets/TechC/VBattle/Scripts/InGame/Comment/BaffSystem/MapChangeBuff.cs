using System.Collections.Generic;
using UnityEngine;

namespace TechC.CommentSystem
{
    /// <summary>
    /// マップ変更バフ。適用されたターゲットのマップをランダムに変更する。
    /// </summary>
    public class MapChangeBuff : BuffBase
    {
        public MapChangeBuff()
        {
            buffName = "MapChangeBuff";
            description = "マップが変化する";
            buffDuration = 7.0f;
            remainingTime = buffDuration;
        }

        /// <summary>
        /// バフが適用されたとき、ランダムでマップを変更する。
        /// </summary>
        public override void Apply(GameObject target)
        {
            base.Apply(target);
            var stageManager = StageManager.I;
            if (stageManager == null) return;
            int currentIndex = stageManager.CurrentStageIndex;
            int stageCount = stageManager.StageCount;

            // 候補のインデックスをリストにする（現在のインデックスを除く）
            List<int> indices = new List<int>();
            for (int i = 0; i < stageCount; i++)
            {
                if (i != currentIndex) indices.Add(i);
            }

            if (indices.Count > 0)
            {
                int randomIndex = indices[Random.Range(0, indices.Count)];
                stageManager.ChangeStage(randomIndex);
            }
            else
            {
                // Debug.LogWarning("ステージが1つしかありません。変更できません。");
            }
        }

        /// <summary>
        /// バフが解除されたとき、マップを非表示にする。
        /// </summary>
        public override void Remove(GameObject target)
        {
            base.Remove(target);
        }

        /// <summary>
        /// バフの残り時間を更新し、終了時にバフを解除する。
        /// </summary>
        public override void UpdateBuff(float deltaTime, GameObject target)
        {
            base.UpdateBuff(deltaTime, target);

            remainingTime -= deltaTime;
            if (remainingTime <= 0f)
            {
                Remove(target);
            }
        }
    }
}
