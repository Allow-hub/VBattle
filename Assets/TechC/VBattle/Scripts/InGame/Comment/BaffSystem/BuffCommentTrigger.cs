using UnityEngine;

namespace TechC.CommentSystem
{
    /// <summary>
    /// バフコメントがプレイヤーと衝突した際にバフを適用するトリガークラス
    /// </summary>
    public class BuffCommentTrigger : MonoBehaviour
    {
        public BuffType buffType;
        [HideInInspector] public string commentText;
        private bool alreadyApplied = false;

        private ObjectPool objectPool;

        /// <summary>
        /// 疑似的なコンストラクタ
        /// </summary>
        /// <param name="objectPool"></param>
        public void Init(ObjectPool objectPool)
        {
            this.objectPool = objectPool;
        }

        /// <summary>
        /// コメントにPlayerが当たったときにバフの効果とエフェクトを発動する
        /// </summary>
        /// <param name="other"></param>

        private void OnTriggerEnter(Collider other)
        {
            if (alreadyApplied) return;
            if (CommentDisplay.I.IsCommentFrozen) return;


            if (other.CompareTag("Player"))
            {
                BuffBase buff = BuffFactory.CreateBuff(buffType);

                if (buff != null)
                {
                    BuffManager buffManager = other.GetComponentInParent<BuffManager>();
                    if (buffManager != null)
                    {
                        buffManager.ApplyBuff(buff);
                    }
                }


                var controller = other.transform.parent.GetComponent<Player.CharacterController>();
                int id = controller.PlayerID;

                float effectTime = buff.remainingTime; /*バフのエフェクトの継続時間にバフの効果の時間を代入 */

                /* バフの種類ごとに適用するエフェクトを変える */
                switch (buffType)
                {
                    case BuffType.Speed:
                        EffectFactory.I.PlayEffect("SpeedComment", id, Quaternion.identity, effectTime);
                        // Debug.Log("SpeedBuffが適用");
                        break;
                    case BuffType.Attack:
                        EffectFactory.I.PlayEffect("AttackComment", id, Quaternion.identity, effectTime);
                        // Debug.Log("AttackBuffが適用");

                        break;
                    default:
                        // Debug.LogWarning($"未対応のバフタイプ: {buffType}");
                        break;
                        // ここでバフを追加可能
                }

                alreadyApplied = true;
                CommentFactory.I.ReturnComment(gameObject);
            }
        }
    }
}
