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
            
            // ObjectPoolでリセットされた状態を上書きして、コメント用に初期化
            obj.transform.localScale = COMMENT_OBJ_SCALE;
            obj.transform.rotation = Quaternion.identity; // 回転をリセット（前回の湾曲で設定された回転をクリア）

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
                commentTrigger.commentText = commentData?.text;
                if (commentData != null && commentData.buffType.HasValue)
                    commentTrigger.buffType = commentData.buffType.Value;
            }
            return obj;
        }

        /// <summary>
        /// コメントをプールに返却
        /// </summary>
        public void ReturnComment(GameObject comment)
        {
            // 子階層の文字オブジェクトをすべてクリア
            // 逆順で処理して、インデックスのずれを防ぐ
            for (int i = comment.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = comment.transform.GetChild(i);
                commentPool.ReturnObject(child.gameObject);
            }

            commentPool.ReturnObject(comment);
        }

        /// <summary>
        /// 文字名に対応する3D文字オブジェクトをプールから取得
        /// </summary>
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
    }
}
