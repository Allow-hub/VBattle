using System.Collections;
using TechC.VBattle.Core.Managers;
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
        /// 位置だけを指定してエフェクトを再生（回転はデフォルト値）
        /// </summary>
        /// <param name="effectName">エフェクト名</param>
        /// <param name="position">エフェクトの位置</param>
        /// <param name="effectRemainingTime">自動返却までの時間（省略可）</param>
        public void PlayEffect(string effectName, int playerID, Quaternion rotation, float effectRemainingTime = 0f)
        {
            /* ObjectPoolから指定された名前のエフェクトを取得 */
            GameObject effect = effectPool.GetObjectByName(effectName);


            /* エフェクトの位置を回転を設定 */
            // var obj = BattleJudge.I.GetPlayerObjById(playerID);
            // effect.transform.position = obj.transform.position;
            // effect.transform.SetParent(obj.transform);
            // effect.transform.rotation = rotation; /* 回転を設定 */
            // effect.SetActive(true); /* エフェクトを表示 */

            // if (effectRemainingTime > 0f)
            // {
            //     StartCoroutine(AutoReturn(effect, effectRemainingTime));
            // }
        }

        public GameObject GetEffectObj(GameObject prefab, Vector3 position, Quaternion rotation) => effectPool.GetObject(prefab, position, rotation);
        public GameObject GetEffectObj(GameObject prefab) => effectPool.GetObject(prefab);
        /// <summary>
        /// 指定時間後にエフェクトをプールに返却する
        /// </summary>
        private IEnumerator AutoReturn(GameObject effect, float delay)
        {
            yield return new WaitForSeconds(delay);

            effectPool.ReturnObject(effect);
        }

        /// <summary>
        /// エフェクトをプールに返却する
        /// </summary>
        /// <param name="effect">返却するエフェクトのゲームオブジェクト</param>
        public void ReturnEffect(GameObject effect)
        {
            effectPool.ReturnObject(effect);
        }
    }
}