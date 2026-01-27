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
        [SerializeField] private CharacterSelectManager characterSelectManager;

        private void Awake()
        {
            selectUIManager.Init();
            startWindow.Init();
            characterSelectManager.Init();
        }
    }
}
