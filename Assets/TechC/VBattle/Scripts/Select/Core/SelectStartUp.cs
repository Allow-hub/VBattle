using TechC.VBattle.Core.Extensions;
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
            // nullチェック
            if (selectUIManager == null) CustomLogger.Error("SelectUIManager が設定されていません");
            if (startWindow == null) CustomLogger.Error("StartWindow が設定されていません");
            if (CharacterSelectManager == null) CustomLogger.Error("CharacterSelectManager が設定されていません");

            // 各シングルトンの初期化を順番に実行
            selectUIManager.Init();
            startWindow.Init();
            CharacterSelectManager.Init();
        }
    }
}
