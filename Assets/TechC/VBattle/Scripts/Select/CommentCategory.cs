using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    [Serializable]
    public class CommentCategory
    {
        [Header("カテゴリ設定")]
        public string categoryName;
        [Range(1, 100)]
        public int categoryWeight = 1;

        [Header("コメントリスト")]
        public List<string> comments = new List<string>();

        [Header("表示設定")]
        public bool isActive = true;
    }
}