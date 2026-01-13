using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.InGame.Comment
{
    public class FreezeAbility : ICommentAbility
    {
        private SpecialCommentTrigger trigger;
        private List<GameObject> chars;


        public void Init(SpecialCommentTrigger trigger)
        {
            this.trigger = trigger;
        }

        public void Release() { }

        public void OnTriggerEnter(Collider collider)
        {
            // コメントを視覚的に非表示にし、左端まで移動させる
            var renderer = trigger.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
            
            // Colliderも無効化して判定を止める
            var boxCollider = trigger.GetComponent<BoxCollider>();
            if (boxCollider != null) boxCollider.enabled = false;
            
            // 子階層の文字も非表示に
            foreach (Transform child in trigger.transform)
            {
                var childRenderer = child.GetComponent<Renderer>();
                if (childRenderer != null) childRenderer.enabled = false;
            }
            
            CommentDisplay.I.OnFreezeTriggered();
        }
    }
}
