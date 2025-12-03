using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.CommentSystem
{
    /// <summary>
    /// コメントの移動処理を担当
    /// </summary>
    [Serializable]
    public class CommentMover
    {
        [Header("コメントを非表示にする場所")]
        [SerializeField] private Transform topLeftDespawn;
        [SerializeField] private Transform buttonLeftDespawn;
        private float despawnPosX;
        private Func<bool> isPausedFunc;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            isPausedFunc = () => BattleJudge.I.IsPaused;
            despawnPosX = topLeftDespawn.transform.position.x;
        }

        /// <summary>
        /// コメント移動処理を開始
        /// </summary>
        public void StartMoving(Transform trans, List<GameObject> chars, SpecialCommentTrigger specialCommentTrigger, Material originalMaterial)
        {
            DelayUtility.StartRepeatedActionWhileWithPause(
                CommentDisplay.I,
                () => trans.gameObject.activeInHierarchy && trans.position.x > despawnPosX,
                Time.fixedDeltaTime,
                isPausedFunc,
                () => MoveCommentFrame(trans, chars, specialCommentTrigger, originalMaterial)
            );
        }

        /// <summary>
        /// 1フレーム分の移動処理
        /// </summary>
        private void MoveCommentFrame(Transform trans, List<GameObject> chars, SpecialCommentTrigger specialCommentTrigger, Material originalMaterial)
        {
            if (!trans.gameObject.activeInHierarchy) return;

            if (CommentDisplay.I.IsCommentFrozen) return;

            trans.position += Vector3.left * CommentDisplay.I.GetCurrentSpeed() * Time.deltaTime;

            if (trans.position.x <= despawnPosX)
            {
                ReturnComment(trans.gameObject, chars);
            }
        }

        /// <summary>
        /// コメントをプールに返却
        /// </summary>
        private void ReturnComment(GameObject comment, List<GameObject> chars)
        {
            CommentDisplay.I.OnCommentReturned(comment);

            foreach (var obj in chars)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    obj.SetActive(false);
                    CommentFactory.I.ReturnChar(obj);
                }
            }

            if (comment.activeInHierarchy)
            {
                comment.SetActive(false);
                CommentFactory.I.ReturnComment(comment);
            }
        }
    }
}
