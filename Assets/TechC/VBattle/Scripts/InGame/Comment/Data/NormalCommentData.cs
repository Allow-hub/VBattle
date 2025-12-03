using UnityEngine;

namespace TechC.CommentSystem
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