using UnityEngine;

namespace TechC.VBattle.InGame.Comment
{
    /// <summary>
    /// 通常コメントを管理する ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NormalCommentData", menuName = "TechC/Comment/Normal")]
    
    public class NormalCommentData : ScriptableObject
    {
        public string [] comment;
    }
}