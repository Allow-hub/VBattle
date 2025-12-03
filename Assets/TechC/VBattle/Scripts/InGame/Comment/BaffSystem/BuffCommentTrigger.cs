using UnityEngine;
using TechC.VBattle.Systems;

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


                var controller = other.transform.parent.GetComponent<TechC.VBattle.InGame.Character.CharacterController>();
                int id = controller.PlayerIndex;

                float effectTime = buff.remainingTime; /*バフのエフェクトの継続時間にバフの効果の時間を代入 */

                switch (buffType)
                {
                    case BuffType.Speed:
                        EffectFactory.I.PlayEffect("SpeedComment", id, Quaternion.identity, effectTime);
                        break;
                    case BuffType.Attack:
                        EffectFactory.I.PlayEffect("AttackComment", id, Quaternion.identity, effectTime);

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
