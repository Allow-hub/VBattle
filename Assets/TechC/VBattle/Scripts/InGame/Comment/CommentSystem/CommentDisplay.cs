using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.CommentSystem
{
    /// <summary>
    /// コメントの表示管理とフリーズ機能を担当
    /// </summary>
    public class CommentDisplay : Singleton<CommentDisplay>
    {
        [SerializeField] private CommentSpawner commentSpawner;
        [SerializeField] private CommentMaterialApplier commentMaterialApplier;
        [SerializeField] private CommentMover commentMover;

        [Header("コメント生成の設定")]
        [SerializeField] private float speed = 100.0f;
        [SerializeField] private float commentInterval = 1.0f;

        [Header("特殊コメントの設定")]
        [SerializeField] private float freezeTime = 3f;

        public bool IsCommentFrozen { get; private set; } = false;

        private bool isSpawning = false;
        private Func<bool> isPausedFunc;

        private List<CommentInfo> activeComments = new List<CommentInfo>();

        /// <summary>
        /// アクティブなコメントの情報を管理するデータクラス
        /// </summary>
        [Serializable]
        public class CommentInfo
        {
            public GameObject commentObject;
            public List<GameObject> characters;
            public Material originalMaterial;
        }

        protected override bool UseDontDestroyOnLoad => false;

        void Start()
        {
            DelayUtility.StartDelayedAction(this, 0f, () =>
            {
                StartCommentSpawning();
            });
        }

        protected override void Init()
        {
            base.Init();
            isPausedFunc = () => BattleJudge.I.IsPaused;
            commentSpawner.Init();
            commentMover.Init();
            commentMaterialApplier.Init();
        }

        /// <summary>
        /// コメントの自動生成を開始
        /// </summary>
        public void StartCommentSpawning()
        {
            if (!isSpawning)
            {
                isSpawning = true;
                StartCoroutine(SpawnCommentWithInterval());
            }
        }

        /// <summary>
        /// 指定したインターバルでコメントを生成
        /// </summary>
        private IEnumerator SpawnCommentWithInterval()
        {
            if (IsCommentFrozen) yield break;

            DelayUtility.StartRepeatedActionWhileWithPause(
                this,
                () => isSpawning,
                commentInterval,
                isPausedFunc,
                () =>
                {
                    GameObject spawnedComment = commentSpawner.SpawnComment();
                    ApplyMaterialToSpawnedComment(spawnedComment);
                }
            );

            yield break;
        }

        /// <summary>
        /// 生成されたコメントにマテリアルを適用
        /// </summary>
        private void ApplyMaterialToSpawnedComment(GameObject comment)
        {
            if (comment == null) return;

            var commentData = commentSpawner.GetLastCommentData();
            var characters = commentSpawner.GetLastCharacters();
            var specialCommentTrigger = comment.GetComponent<SpecialCommentTrigger>();

            Material targetMaterial = commentMaterialApplier.GetCommentMaterial(commentData.type);

            activeComments.Add(new CommentInfo
            {
                commentObject = comment,
                characters = characters,
                originalMaterial = targetMaterial
            });

            commentMaterialApplier.ApplyMaterialToCharacters(characters, targetMaterial);
            commentMover.StartMoving(comment.transform, characters, specialCommentTrigger, targetMaterial);
        }

        /// <summary>
        /// フリーズコメント取得時に呼ばれる
        /// </summary>
        public void OnFreezeTriggered()
        {
            if (!IsCommentFrozen)
            {
                IsCommentFrozen = true;
                ApplyFreezeEffectToAllComments();
                DelayUtility.StartDelayedActionWithPause(
                    this,
                    freezeTime,
                    () => BattleJudge.I.IsPaused,
                    () =>
                    {
                        IsCommentFrozen = false;
                        RestoreOriginalMaterials();
                    });
            }
        }

        private void ApplyFreezeEffectToAllComments()
        {
            CleanupInactiveComments();
            foreach (var commentInfo in activeComments)
            {
                commentMaterialApplier.ApplyFreezeEffectToCharacters(commentInfo.characters, commentInfo.originalMaterial);
            }
        }

        private void RestoreOriginalMaterials()
        {
            CleanupInactiveComments();
            foreach (var commentInfo in activeComments)
            {
                commentMaterialApplier.ApplyMaterialToCharacters(commentInfo.characters, commentInfo.originalMaterial);
            }
        }

        /// <summary>
        /// 非アクティブなコメントをリストから削除
        /// </summary>
        private void CleanupInactiveComments()
        {
            for (int i = activeComments.Count - 1; i >= 0; i--)
            {
                if (activeComments[i].commentObject == null ||
                    !activeComments[i].commentObject.activeInHierarchy)
                {
                    activeComments.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// コメント返却時の通知
        /// </summary>
        public void OnCommentReturned(GameObject comment)
        {
            for (int i = activeComments.Count - 1; i >= 0; i--)
            {
                if (activeComments[i].commentObject == comment)
                {
                    activeComments.RemoveAt(i);
                    break;
                }
            }
        }

        public float GetCurrentSpeed() => speed;
        public void SetSpeed(float newSpeed) => speed = newSpeed;
        public void AddSpeed(float amount) => speed += amount;
    }
}