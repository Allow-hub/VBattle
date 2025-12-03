using System.Collections.Generic;
using UnityEngine;

namespace TechC.CommentSystem
{
    /// <summary>
    /// 特殊コメントの当たり判定を取り、Inspectorで設定した能力リストを実行するクラス
    /// </summary>
    public class SpecialCommentTrigger : MonoBehaviour
    {
        [SerializeReference]
        public List<ICommentAbility> abilities = new();

        private void Awake()
        {
            // 各アビリティを初期化
            foreach (var ability in abilities)
            {
                ability?.Init(this);
            }
        }

        private void OnDestroy()
        {
            // abilitiesの各要素にReleaseを呼ぶ
            foreach (var ability in abilities)
            {
                ability?.Release();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            foreach (var ability in abilities)
            {
                ability?.OnTriggerEnter(other);
            }
        }
    }
}
