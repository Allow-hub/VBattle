using UnityEngine;

namespace TechC.VBattle.InGame.Comment
{
    /// <summary>
    /// バフコメントデータを格納するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "BuffCommentData", menuName = "TechC/Comment/Buff")]
    public class BuffCommentData : ScriptableObject
    {
        [Header("バフの種類")]
        public BuffType buffType;
        public string[] comments;
    }
}
