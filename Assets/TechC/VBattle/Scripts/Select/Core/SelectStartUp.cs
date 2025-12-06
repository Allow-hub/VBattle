using UnityEngine;

namespace TechC.VBattle.Select.Core
{
    /// <summary>
    /// Selectシーンのシングルトン初期化順序を決める
    /// </summary>
    [DefaultExecutionOrder(-9999)]
    public class SelectStartUp : MonoBehaviour
    {
        [Header("セレクトシーンのシングルトン管理")]
        [SerializeField] private SelectUIManager selectUIManager;
        [SerializeField] private StartWindow startWindow;
        [SerializeField] private CharacterSelectManager CharacterSelectManager;

        private void Awake()
        {
            // 各シングルトンの初期化を順番に実行
            selectUIManager.Init();
            startWindow.Init();
            CharacterSelectManager.Init();
        }
    }
}
