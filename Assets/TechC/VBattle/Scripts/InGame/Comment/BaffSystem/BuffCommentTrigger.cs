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

        private void OnEnable()
        {
            // Pool から取得時に状態をリセット
            alreadyApplied = false;
            
            var renderer = GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = true;
            
            var boxCollider = GetComponent<BoxCollider>();
            if (boxCollider != null) boxCollider.enabled = true;
        }


        /// <summary>
        /// コメントにPlayerが当たったときにバフの効果とエフェクトを発動する
        /// </summary>
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

                float effectTime = buff.remainingTime;

                switch (buffType)
                {
                    case BuffType.Speed:
                        EffectFactory.I.PlayEffect("SpeedBuff", other.gameObject, Quaternion.identity, effectTime);
                        break;
                    case BuffType.Attack:
                        EffectFactory.I.PlayEffect("AttackBuff", other.gameObject, Quaternion.identity, effectTime);
                        break;
                    default:
                        break;
                }

                alreadyApplied = true;
                
                // コメントを視覚的に非表示にし、左端まで移動させる
                var renderer = GetComponent<Renderer>();
                if (renderer != null) renderer.enabled = false;
                
                // Colliderも無効化して判定を止める
                var boxCollider = GetComponent<BoxCollider>();
                if (boxCollider != null) boxCollider.enabled = false;
                
                // 子階層の文字も非表示に
                foreach (Transform child in transform)
                {
                    var childRenderer = child.GetComponent<Renderer>();
                    if (childRenderer != null) childRenderer.enabled = false;
                }
            }
        }
    }
}
