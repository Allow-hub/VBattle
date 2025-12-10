using System;
using TechC.VBattle.InGame.Character;

namespace TechC.VBattle.InGame.Comment
{
    /// <summary>
    /// コメントアビリティの発動を管理
    /// CharacterControllerとCommentSystemの仲介役
    /// </summary>
    public class CommentAbilityHandler
    {
        private CharacterController owner;
        private Action pendingAbility;

        public CommentAbilityHandler(CharacterController controller)
        {
            owner = controller;
        }

        /// <summary>
        /// コメントアビリティを登録（CommentSystemから呼ばれる）
        /// </summary>
        public void RegisterAbility(Action ability)
        {
            pendingAbility = ability;
        }

        /// <summary>
        /// 登録されたアビリティを実行（CharacterController内部から呼ばれる）
        /// </summary>
        public void ExecutePendingAbility()
        {
            pendingAbility?.Invoke();
            pendingAbility = null;
        }

        /// <summary>
        /// 保留中のアビリティがあるか
        /// </summary>
        public bool HasPendingAbility() => pendingAbility != null;

        /// <summary>
        /// クリーンアップ
        /// </summary>
        public void Dispose()
        {
            pendingAbility = null;
            owner = null;
        }
    }
}
