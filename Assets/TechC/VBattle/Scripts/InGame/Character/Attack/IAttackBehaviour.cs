using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 攻撃の各機能を持ったコンポーネントが継承するインターフェース
    /// </summary>
    public interface IAttackBehaviour
    {
        /// <summary>初期化</summary>
        /// <param name="owner">主体のオブジェクト</param>
        void Initialize(GameObject owner);
        void Activate(GameObject character) { }
        /// <summary>更新処理</summary>
        /// <param name="deltaTime">一回の更新の秒数</param>
        void OnUpdate(float deltaTime);

        /// <summary>解放</summary>
        void OnRelease();
        /// <summary>トリガー開始時の処理</summary>
        void OnTriggerEnter(Collider other) { }

        /// <summary>トリガー継続時の処理</summary>
        void OnTriggerStay(Collider other) { }

        /// <summary>トリガー終了時の処理</summary>
        void OnTriggerExit(Collider other) { }
    }
}