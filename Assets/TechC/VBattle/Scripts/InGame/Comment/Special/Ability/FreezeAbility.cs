using System.Collections.Generic;
using UnityEngine;

namespace TechC.CommentSystem
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
            ReturnCommentAndChars(trigger.gameObject, chars);
            CommentDisplay.I.OnFreezeTriggered();
        }

        /// <summary>
        /// コメント本体と文字オブジェクトをプールに返却する
        /// </summary>
        public void ReturnCommentAndChars(GameObject comment, List<GameObject> chars)
        {
            if (chars != null)
            {
                foreach (var obj in chars)
                {
                    if (obj != null && obj.activeInHierarchy)
                    {
                        CommentFactory.I.ReturnChar(obj);
                    }
                }
            }
            if (comment != null && comment.activeInHierarchy)
            {
                CommentFactory.I.ReturnComment(comment);
            }
        }
    }
}
