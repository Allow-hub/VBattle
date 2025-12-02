using TechC.VBattle.Core.Managers;
using UnityEngine;

namespace TechC.VBattle.Systems
{
    /// <summary>
    /// キャラクターの攻撃エフェクト生成を管理するファクトリクラス
    /// AttackObjectControllerを持つオブジェクトはここで生成する
    /// </summary>
    public class CharaAttackFactory : Singleton<CharaAttackFactory>
    {
        [SerializeField] private ObjectPool objectPool;
        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
        }

        /// <summary>
        /// エフェクトを取得
        /// </summary>
        /// <param name="prefab">エフェクトのプレハブ</param>
        /// <returns></returns>
        public GameObject GetAttackObj(GameObject prefab) => objectPool.GetObject(prefab);

        /// <summary>
        /// エフェクトを位置と回転を指定したうえで取得
        /// </summary>
        /// <param name="prefab">エフェクトのプレハブ</param>
        /// <param name="position">生成位置</param>
        /// <param name="rotation">初期回転</param>
        /// <returns></returns>
        public GameObject GetAttackObj(GameObject prefab, Vector3 position, Quaternion rotation) => objectPool.GetObject(prefab, position, rotation);

        public void ReturnAttackObj(GameObject obj) => objectPool.ReturnObject(obj);
    }
}
