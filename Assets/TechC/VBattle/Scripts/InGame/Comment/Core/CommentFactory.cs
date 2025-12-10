using UnityEngine;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Systems;
using TechC.VBattle.Core.Extensions;

namespace TechC.VBattle.InGame.Comment
{
    public class CommentFactory : Singleton<CommentFactory>
    {
        [SerializeField] private ObjectPool commentPool;

        [Header("文字とそのPrefabのScriptableObject")]
        [SerializeField] private CharPrefabDatabase charPrefabDatabase;
        protected override bool UseDontDestroyOnLoad => false;

        // 3DText用のスケール定数
        private static readonly Vector3 COMMENT_OBJ_SCALE = new Vector3(0.25f, 0.25f, 0.25f);

        /// <summary>
        /// コメントを取得する
        /// </summary>
        /// <param name="commentData"></param>
        /// <param name="commentPrefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public GameObject GetComment(CommentData commentData, GameObject commentPrefab)
        {
            GameObject obj = commentPool.GetObject(commentPrefab);
            obj.transform.localScale = COMMENT_OBJ_SCALE;

            if (commentData.type == CommentType.Normal) return obj; // NormalはBuffCommentTriggerがついていないため早期reture

            if (commentData != null && (commentData.type == CommentType.Grass || commentData.type == CommentType.Freeze))
            {
                var specialCommentTrigger = obj.GetComponent<SpecialCommentTrigger>();
                if (specialCommentTrigger == null)
                    CustomLogger.Error("SpecialCommentTriggerがPrefabにアタッチされていません。PrefabのInspectorで必ず追加してください。");
                
            }
            else
            {
                var commentTrigger = obj.GetComponent<BuffCommentTrigger>();
                commentTrigger.Init(commentPool);
                commentTrigger.commentText = commentData?.text;
                if (commentData != null && commentData.buffType.HasValue)
                    commentTrigger.buffType = commentData.buffType.Value;
            }
            return obj;
        }

        public void ReturnComment(GameObject comment)
        {
            commentPool.ReturnObject(comment);
        }

        public GameObject GetChar(string charName)
        {

            GameObject charPrefab = null;
            foreach (var entry in charPrefabDatabase.entries)
            {
                if (entry.charText == charName)
                {
                    charPrefab = entry.charPrefab;
                    break;
                }
            }

            if (charPrefab == null)
            {
                CustomLogger.Error($"その文字はcharPrefabDatabaseに登録されていません: {charName}");
                return null;
            }

            // ObjectPoolから取得
            GameObject charObj = commentPool.GetObject(charPrefab);
            return charObj;
        }

        // 文字オブジェクトをプールに返却
        public void ReturnChar(GameObject charObj)
        {
            commentPool.ReturnObject(charObj);
        }
    }
}
