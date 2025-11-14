using System;
using UnityEngine;

namespace TechC.VBattle.Systems
{
    /// <summary>
    /// オブジェクトプールの各アイテムを定義するクラス
    /// </summary>
    [Serializable]
    public class ObjectPoolItem
    {
        [Tooltip("プール識別用の名前")]
        public string name;
        
        [Tooltip("プールするプレハブ")]
        public GameObject prefab;
        
        [Tooltip("生成されたオブジェクトを格納する親オブジェクト")]
        public GameObject parent;
        
        [Tooltip("初期プールサイズ")]
        [Range(0, 1000)]
        public int initialSize = 5;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public ObjectPoolItem() { }

        /// <summary>
        /// すべてのパラメータを指定するコンストラクタ
        /// </summary>
        /// <param name="name">プール識別用の名前</param>
        /// <param name="prefab">プールするプレハブ</param>
        /// <param name="parent">生成されたオブジェクトを格納する親オブジェクト</param>
        /// <param name="initialSize">初期プールサイズ</param>
        public ObjectPoolItem(string name, GameObject prefab, GameObject parent, int initialSize)
        {
            this.name = name;
            this.prefab = prefab;
            this.parent = parent;
            this.initialSize = Mathf.Max(0, initialSize); // 負の値を防止
        }

        /// <summary>
        /// プレハブと親オブジェクトのnullチェックを行います
        /// </summary>
        /// <returns>有効なプール項目の場合はtrue</returns>
        public bool IsValid()
        {
            return prefab != null && parent != null;
        }
    }
}