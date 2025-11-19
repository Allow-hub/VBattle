using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    // 攻撃セットScriptableObject - 複数の攻撃をグループ化
    [CreateAssetMenu(fileName = "AttackSet", menuName = "TechC/Combat/Attack Set")]
    public class AttackSet : ScriptableObject
    {
        [Header("キャラクター情報")]
        public CharaName charaName;
        [Header("攻撃データ一覧")]
        public List<AttackEntry> attacks;
        public Dictionary<(AttackDirection, AttackType), AttackData> attackDataMap;
        private void OnEnable()
        {
            attackDataMap = new Dictionary<(AttackDirection, AttackType), AttackData>();

            foreach (var entry in attacks)
            {
                var key = (entry.direction, entry.type);
                if (!attackDataMap.ContainsKey(key))
                {
                    attackDataMap.Add(key, entry.attackData);
                }
                else
                {
                    Debug.LogWarning($"Duplicate key in AttackSet: {key}");
                }
            }
        }


    }
    [System.Serializable]
    public struct AttackEntry
    {
        public string attackName;
        public AttackType type;
        public AttackDirection direction;
        public AttackData attackData;
    }
}