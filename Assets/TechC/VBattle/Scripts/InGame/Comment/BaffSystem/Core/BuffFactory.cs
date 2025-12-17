using System.Collections.Generic;
using UnityEngine;
using TechC.VBattle.Core.Managers;

namespace TechC.VBattle.InGame.Comment
{
    /// <summary>
    /// バフを生成するファクトリクラス
    /// バフタイプに応じて、適切なバフを生成するためのメソッドを提供
    /// </summary>
    public class BuffFactory : Singleton<BuffFactory>
    {
        private Dictionary<BuffType, System.Func<BuffBase>> buffDictionary;
        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
            /* 初期化 */
            buffDictionary = new Dictionary<BuffType, System.Func<BuffBase>>()
            {
                { BuffType.Speed, () => new SpeedBuff()},
                { BuffType.Attack, () => new AttackBuff()}
            };
        }

        public BuffBase CreateBuff(BuffType buffType)
        {
            /* Dictionaryにバフタイプが登録されていなければ、それに対応するバフを生成 */
            if (buffDictionary.ContainsKey(buffType))
                return buffDictionary[buffType]();
            else
                return null;
        }
    }
}

