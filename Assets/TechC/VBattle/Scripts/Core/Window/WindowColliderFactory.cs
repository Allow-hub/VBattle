using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Util;
using TechC.VBattle.Systems;
using UnityEngine;

namespace TechC.VBattle.Core.Window
{
    /// <summary>
    /// ウィンドウに当たり判定をつけるためのファクトリー
    /// </summary>
    public class WindowColliderFactory : Singleton<WindowColliderFactory>
    {
        [SerializeField] private ObjectPool objectPool;
        [SerializeField] private GameObject colliderPrefab;
        protected override bool UseDontDestroyOnLoad => false;

        /// <summary>
        /// ウィンドウ用コライダーを取得
        /// </summary>
        /// <returns></returns>
        public GameObject GetWindowColliderPrefab()
        {
            var obj = objectPool.GetObject(colliderPrefab);
            if (obj == null)
            {
                CustomLogger.Error("obj not found.", LogTagUtil.TagWidnow);
                return null;
            }
            return obj;
        }

        /// <summary>
        /// ウィンドウ用コライダーを返却
        /// </summary>
        /// <param name="obj"></param>
        public void ReturnWindowCollider(GameObject obj) => objectPool.ReturnObject(obj);
    }
}