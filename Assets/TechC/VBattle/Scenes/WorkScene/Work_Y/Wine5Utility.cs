#if UNITY_EDITOR
using UnityEngine.SceneManagement;

namespace TechC.VBattle.Wine5
{
    /// <summary>
    /// Wine5の開発・テスト用ユーティリティクラス
    /// エディタ専用で本番ビルドには含まれない
    /// </summary>
    public static class Wine5Utility
    {
        /// <summary>
        /// テスト用のシーン切り替え（Battle直接起動など）
        /// </summary>
        public static void LoadTestScene(string sceneName) =>
            SceneManager.LoadSceneAsync(sceneName);
    }
}
#endif