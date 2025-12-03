using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.InGame.Comment
{
    /// <summary>
    /// ランダムにコメントを選び提供する
    /// </summary>
    public class CommentProvider : MonoBehaviour
    {
        [Header("コメントデータ")]
        public NormalCommentData normalComments;
        public List<BuffCommentData> buffComments;
        public List<SpecialCommentData> specialComments;

        [Header("コメントの出現確率")]
        [SerializeField, Range(0f, 1f)] private float normalChance = 0.7f;
        [SerializeField, Range(0f, 1f)] private float speedBuffChance = 0.2f;
        [SerializeField, Range(0f, 1f)] private float attackBuffChance = 0.2f;
        [SerializeField, Range(0f, 1f)] private float grassCommentChance = 0.1f;
        [SerializeField, Range(0f, 1f)] private float freezeCommentChance = 0.1f;

        private List<BuffCommentData> speedBuffs;
        private List<BuffCommentData> attackBuffs;
        private List<SpecialCommentData> specialCommentList;
        private float totalChance;
        private List<SpecialCommentData.SpecialCommentEntry> grassEntries = new List<SpecialCommentData.SpecialCommentEntry>();
        private List<SpecialCommentData.SpecialCommentEntry> freezeEntries = new List<SpecialCommentData.SpecialCommentEntry>();

        private void Awake()
        {
            totalChance = normalChance + speedBuffChance + attackBuffChance + grassCommentChance + freezeCommentChance;

            // 確率が0またはマイナスならデフォルト値に設定
            if (totalChance <= 0f)
            {
                normalChance = 0.7f;
                speedBuffChance = 0.1f;
                attackBuffChance = 0.1f;
                grassCommentChance = 0.05f;
                freezeCommentChance = 0.05f;
                totalChance = 1.0f;
            }

            // buffCommentsを事前にフィルタリングして分類
            speedBuffs = new List<BuffCommentData>();
            attackBuffs = new List<BuffCommentData>();
            specialCommentList = new List<SpecialCommentData>();

            foreach (var buff in buffComments)
            {
                if (buff.buffType == BuffType.Speed)
                {
                    speedBuffs.Add(buff);
                }
                else if (buff.buffType == BuffType.Attack)
                {
                    attackBuffs.Add(buff);
                }
            }

            // SpecialCommentDataからGrass/Freezeコメントを分類
            grassEntries.Clear();
            freezeEntries.Clear();
            if (specialComments != null)
            {
                foreach (var so in specialComments)
                {
                    if (so.comments != null)
                    {
                        foreach (var entry in so.comments)
                        {
                            // コメント内容で判定（例: "草"を含むならGrass, "固定"や"フリーズ"を含むならFreeze）
                            if (entry.comment.Contains("草"))
                                grassEntries.Add(entry);
                            else if (entry.comment.Contains("固定") || entry.comment.Contains("フリーズ"))
                                freezeEntries.Add(entry);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ランダムなコメントを取得するメソッド
        /// </summary>
        /// <returns></returns>
        public CommentData GetRandomComment()
        {
            // ランダムな値を計算
            float randomValue = Random.value * totalChance;

            float threshold = 0f;
            // grass/freezeは個別に抽選するため合算しない

            // 通常コメント
            threshold += normalChance;
            if (randomValue < threshold)
            {
                string text = normalComments.comment[Random.Range(0, normalComments.comment.Length)];
                return new CommentData(CommentType.Normal, text, null);
            }

            // Speedバフコメント
            threshold += speedBuffChance;
            if (randomValue < threshold)
            {
                if (speedBuffs.Count > 0)
                {
                    var buff = speedBuffs[Random.Range(0, speedBuffs.Count)];
                    string text = buff.comments[Random.Range(0, buff.comments.Length)];
                    return new CommentData(CommentType.SpeedBuff, text, buff.buffType);
                }
            }

            // Attackバフコメント
            threshold += attackBuffChance;
            if (randomValue < threshold)
            {
                if (attackBuffs.Count > 0)
                {
                    var buff = attackBuffs[Random.Range(0, attackBuffs.Count)];
                    string text = buff.comments[Random.Range(0, buff.comments.Length)];
                    return new CommentData(CommentType.AttackBuff, text, buff.buffType);
                }
            }

            // Grassコメント
            threshold += grassCommentChance;
            if (randomValue < threshold)
            {
                if (grassEntries.Count > 0)
                {
                    var entry = grassEntries[Random.Range(0, grassEntries.Count)];
                    return new CommentData(CommentType.Grass, entry.comment, null);
                }
            }

            // Freezeコメント
            threshold += freezeCommentChance;
            if (randomValue < threshold)
            {
                if (freezeEntries.Count > 0)
                {
                    var entry = freezeEntries[Random.Range(0, freezeEntries.Count)];
                    return new CommentData(CommentType.Freeze, entry.comment, null);
                }
            }

            // fallback（通常コメント）
            string fallback = normalComments.comment[Random.Range(0, normalComments.comment.Length)];
            return new CommentData(CommentType.Normal, fallback, null);
        }

#if UNITY_EDITOR
        [ContextMenu("全ての出現確率を0にする")]
        private void SetAllChancesToZero()
        {
            normalChance = 0f;
            speedBuffChance = 0f;
            attackBuffChance = 0f;
            grassCommentChance = 0f;
            freezeCommentChance = 0f;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
