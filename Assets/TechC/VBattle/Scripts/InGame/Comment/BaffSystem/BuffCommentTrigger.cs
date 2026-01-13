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
        
        [Header("エフェクト設定")]
        [SerializeField] private GameObject effectPrefab;
        
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
                    
                    // エフェクト再生
                    if (effectPrefab != null)
                        EffectFactory.I.PlayEffect(effectPrefab, other.gameObject, Quaternion.identity, buff.remainingTime);
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
