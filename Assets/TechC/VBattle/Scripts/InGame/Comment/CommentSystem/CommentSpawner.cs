using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.CommentSystem
{
    [Serializable]
    public class CommentSpawner
    {
        [SerializeField] private CommentProvider commentProvider;

        [Header("コメントのテキスト用Prefab")]
        [SerializeField] private GameObject commentPrefab;
        [SerializeField] private GameObject speedBuffPrefab;
        [SerializeField] private GameObject attackBuffPrefab;
        [SerializeField] private GameObject mapChangePrefab;
        [SerializeField] private GameObject grassPrefab;
        [SerializeField] private GameObject freezePrefab;

        [Header("コメントが出現する場所")]
        [SerializeField] private Transform topRightSpawnPos;
        [SerializeField] private Transform bottomRightSpawnPos;
        private float topRightSpawnPosY = 5.0f;
        private float bottomRightSpawnPosY = -5.0f;
        private float spawnPosX = 10.0f;

        private const float PLAYER_TOP_OFFSET = -5.3f;
        private bool isInitialized = false;

        // 最後に生成されたコメントのデータを保持
        private CommentData lastCommentData;
        private List<GameObject> lastCharacters;

        public void Init()
        {
            topRightSpawnPosY = topRightSpawnPos.position.y;
            bottomRightSpawnPosY = bottomRightSpawnPos.position.y;
            spawnPosX = topRightSpawnPos.position.x;
            isInitialized = true;
        }

        /// <summary>
        /// コメントをcommentProviderを通じて発生させる処理
        /// </summary>
        /// <returns>生成されたコメントのGameObject</returns>
        public GameObject SpawnComment()
        {
            if (!isInitialized)
            {
                Init();
            }
            
            if (CommentDisplay.I != null && CommentDisplay.I.IsCommentFrozen) return null;

            var commentData = commentProvider.GetRandomComment();
            GameObject comment = CommentFactory.I.GetComment(commentData, GetCommentPrefab(commentData));

            // 文字オブジェクトを生成
            List<GameObject> spawnedChars = AllCharacterHelper.ProcessCommentText(commentData.text, comment.transform, Color.white);

            // 位置を設定
            float randomY = UnityEngine.Random.Range(bottomRightSpawnPosY, topRightSpawnPosY);
            comment.transform.position = new Vector3(spawnPosX, randomY, PLAYER_TOP_OFFSET);

            // 最後に生成されたデータを保存
            lastCommentData = commentData;
            lastCharacters = spawnedChars;

            return comment;
        }

        /// <summary>
        /// 最後に生成されたコメントのデータを取得
        /// </summary>
        public CommentData GetLastCommentData()
        {
            return lastCommentData;
        }

        /// <summary>
        /// 最後に生成された文字オブジェクトを取得
        /// </summary>
        public List<GameObject> GetLastCharacters()
        {
            return lastCharacters;
        }

        private GameObject GetCommentPrefab(CommentData commentData)
        {
            switch (commentData.type)
            {
                case CommentType.Normal:
                    return commentPrefab;
                case CommentType.AttackBuff:
                    return attackBuffPrefab;
                case CommentType.MapChange:
                    return mapChangePrefab;
                case CommentType.SpeedBuff:
                    return speedBuffPrefab;
                case CommentType.Grass:
                    return grassPrefab;
                case CommentType.Freeze:
                    return freezePrefab;
                default:
                    return commentPrefab;
            }
        }
    }
}
