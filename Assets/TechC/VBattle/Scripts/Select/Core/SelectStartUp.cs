using TechC.VBattle.Core.Extensions;
using UnityEngine;

namespace TechC.Select
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
        [SerializeField] private CharacterSelectManagerFix characterSelectManagerFix;

        private void Awake()
        {
            // 各シングルトンの初期化を順番に実行
            
            if (selectUIManager != null)
            {
                CustomLogger.Info($"selectUIManagerがnullです");
                selectUIManager.Init();
            }
            
            // 将来的に使用される可能性があるシングルトン
            if (startWindow != null)
            {
                CustomLogger.Info($"startWindowがnullです");
                startWindow.Init();
            }
            
            if (characterSelectManagerFix != null)
            {
                characterSelectManagerFix.Init();
                CustomLogger.Info($"characterSelectManagerFixがnullです");
            }
        }
    }
}
