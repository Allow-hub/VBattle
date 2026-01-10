using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Util;
using UnityEngine;

namespace TechC.VBattle.Systems
{

    /// <summary>
    /// エフェクトを生成するファクトリ（シングルトン）
    /// </summary>
    public class EffectFactory : Singleton<EffectFactory>
    {
        [SerializeField]
        private ObjectPool effectPool;

        protected override bool UseDontDestroyOnLoad => false;
        public override void Init()
        {
            base.Init();
            effectPool.ForEachInactiveInPool(obj =>
            {
                // var charaEffect = obj.GetComponent<CharaEffect>();
                // charaEffect?.Init(effectPool);
            });
        }

        /// <summary>
        /// エフェクトを再生
        /// </summary>
        /// <param name="effectName">エフェクト名</param>
        /// <param name="playerObj">エフェクトを配置するプレイヤーオブジェクト</param>
        /// <param name="rotation">エフェクトの回転</param>
        /// <param name="effectRemainingTime">自動返却までの時間（省略可）</param>
        public void PlayEffect(string effectName, GameObject playerObj, Quaternion rotation, float effectRemainingTime = 0f)
        {
            // ObjectPoolから指定された名前のエフェクトを取得
            GameObject effect = effectPool.GetObjectByName(effectName);
            
            if (effect == null)
            {
                Debug.LogWarning($"エフェクト '{effectName}' が見つかりません。");
                return;
            }

            // エフェクトの位置と回転を設定
            if (playerObj != null)
            {
                effect.transform.position = playerObj.transform.position;
                effect.transform.SetParent(playerObj.transform);
            }
            effect.transform.rotation = rotation;
            effect.SetActive(true);

            // 指定時間後に自動返却
            if (effectRemainingTime > 0f)
            {
                _ = DelayUtility.StartDelayedActionAsync(effectRemainingTime, () =>
                {
                    if (effect != null)
                        effectPool.ReturnObject(effect);
                });
            }
        }

        public GameObject GetEffectObj(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (effectPool == null)
            {
                Debug.LogError("EffectFactory: effectPool is not assigned!");
                return null;
            }
            
            if (prefab == null)
            {
                Debug.LogError("EffectFactory: prefab is null!");
                return null;
            }
            
            GameObject result = effectPool.GetObject(prefab, position, rotation);
            if (result == null)
            {
                Debug.LogWarning($"EffectFactory: Failed to get object from pool for prefab '{prefab.name}'. Make sure the prefab is registered in ObjectPool.");
            }
            
            return result;
        }
        
        public GameObject GetEffectObj(GameObject prefab) 
        {  
            GameObject result = effectPool.GetObject(prefab);    
            return result;
        }

        /// <summary>
        /// エフェクトをプールに返却する
        /// </summary>
        public void ReturnEffect(GameObject effect)
        {
            effectPool.ReturnObject(effect);
        }
    }
}