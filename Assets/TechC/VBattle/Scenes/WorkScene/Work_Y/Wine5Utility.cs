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
        /// 自分用のシーン切り替え
        /// </summary>
        public static void LoadMyScene() =>
            SceneManager.LoadSceneAsync("WorkScene_Y");
    }
}
#endif