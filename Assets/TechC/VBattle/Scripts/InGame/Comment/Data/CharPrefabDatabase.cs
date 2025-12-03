using System.Collections.Generic;
using UnityEngine;

namespace TechC.CommentSystem
{
    [System.Serializable]
    public class CharPrefabEntry
    {
        public string charText;
        public GameObject charPrefab;
    }

    [CreateAssetMenu(fileName = "CharPrefabDatabase", menuName = "TechC/Comment/3DCharDatabase")]
    public class CharPrefabDatabase : ScriptableObject
    {
        public List<CharPrefabEntry> entries = new List<CharPrefabEntry>();
    }
}
