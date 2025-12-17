using UnityEngine;
using TechC.VBattle.Systems;

namespace TechC.VBattle.InGame.Comment
{
    /// <summary>
    /// バフコメントがプレイヤーと衝突した際にバフを適用するトリガークラス
    /// </summary>
    public class BuffCommentTrigger : MonoBehaviour
    {
        public BuffType buffType;
        [HideInInspector] public string commentText;
        private bool alreadyApplied = false;


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
                BuffBase buff = BuffFactory.I.CreateBuff(buffType);

                if (buff != null)
                {
                    BuffManager buffManager = other.GetComponent<BuffManager>();
                    if (buffManager != null)
                        buffManager.ApplyBuff(buff);
                }

                var controller = other.transform.GetComponent<Character.CharacterController>();
                int id = controller.PlayerIndex;

                float effectTime = buff.remainingTime;

                switch (buffType)
                {
                    case BuffType.Speed:
                        EffectFactory.I.PlayEffect("SpeedBuff", id, Quaternion.identity, effectTime);
                        break;
                    case BuffType.Attack:
                        EffectFactory.I.PlayEffect("AttackBuff", id, Quaternion.identity, effectTime);
                        break;
                    default:
                        break;
                }

                alreadyApplied = true;
                CommentFactory.I.ReturnComment(gameObject);
            }
        }
    }
}
