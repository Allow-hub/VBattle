using UnityEngine;
using System;

namespace TechC.VBattle.InGame.Comment
{
    /// <summary>
    /// 特殊コメントを管理する ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "SpecialCommentData", menuName = "TechC/Comment/Special")]

    public class SpecialCommentData : ScriptableObject
    {
        [Serializable]
        public class SpecialCommentEntry
        {
            public string comment;
            public GameObject prefab;
        }

        public SpecialCommentEntry[] comments;
    }
}
