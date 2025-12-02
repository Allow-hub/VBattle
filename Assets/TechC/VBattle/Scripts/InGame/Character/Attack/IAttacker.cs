using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 攻撃を行うことができるエンティティのインターフェース
    /// キャラクター、飛び道具など全ての攻撃源に実装
    /// </summary>
    public interface IAttacker
    {
        /// <summary>
        /// 攻撃者のGameObject
        /// </summary>
        /// <value></value>
        GameObject GameObject { get; }

        /// <summary>
        /// 攻撃者のTransform
        /// </summary>
        /// <value></value>
        Transform Transform { get; }

        /// <summary>
        /// 攻撃の所有者（飛び道具の場合は発射したキャラクター）
        /// </summary>
        /// <value></value>
        CharacterController Owner { get; }
    }
}