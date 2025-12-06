using UnityEngine;

namespace Allow
{
    /// <summary>
    /// 攻撃の各機能を持ったコンポーネントが継承するインターフェース
    /// </summary>
    public interface IExampleBehaviour
    {
        /// <summary>初期化</summary>
        /// <param name="owner">主体のオブジェクト</param>
        void Initialize(GameObject owner);

        /// <summary>更新処理</summary>
        /// <param name="deltaTime">一回の更新の秒数</param>
        void OnUpdate(float deltaTime);

        /// <summary>解放</summary>
        void OnRelease();
    }
}