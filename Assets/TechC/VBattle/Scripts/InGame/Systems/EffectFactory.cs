using TechC.VBattle.Core.Extensions;
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
        [SerializeField] private ObjectPool effectPool;
        [SerializeField] private GameObject debrisEffectPrefab;
        public GameObject DebrisEffectPrefab => debrisEffectPrefab; 

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
        /// <param name="effectPrefab">エフェクトPrefab</param>
        /// <param name="playerObj">エフェクトを配置するプレイヤーオブジェクト</param>
        /// <param name="rotation">エフェクトの回転</param>
        /// <param name="effectRemainingTime">自動返却までの時間（省略可）</param>
        public void PlayEffect(GameObject effectPrefab, GameObject playerObj, Quaternion rotation, float effectRemainingTime = 0f)
        {
            // ObjectPoolから指定されたPrefabのエフェクトを取得
            GameObject effect = effectPool.GetObject(effectPrefab);
            
            if (effect == null)
            {
                CustomLogger.Warning($"エフェクト '{effectPrefab?.name}' が見つかりません。");
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
                CustomLogger.Error("EffectFactory: effectPool is not assigned!");
                return null;
            }
            
            if (prefab == null)
            {
                CustomLogger.Error("EffectFactory: prefab is null!");
                return null;
            }
            
            GameObject result = effectPool.GetObject(prefab, position, rotation);
            if (result == null)
                CustomLogger.Warning($"EffectFactory: Failed to get object from pool for prefab '{prefab.name}'. Make sure the prefab is registered in ObjectPool.");
            
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