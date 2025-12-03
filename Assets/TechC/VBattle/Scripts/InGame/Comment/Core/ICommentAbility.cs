using UnityEngine;

namespace TechC.CommentSystem
{
    /// <summary>
    /// 特殊コメントの能力を表すインターフェース。
    /// 各種特殊効果（固定・草生成など）を実装し、SpecialCommentTriggerから呼び出される。
    /// </summary>
    public interface ICommentAbility
    {
        void Init(SpecialCommentTrigger trigger);
        void Release();
        void OnTriggerEnter(Collider collider);
    }
}
