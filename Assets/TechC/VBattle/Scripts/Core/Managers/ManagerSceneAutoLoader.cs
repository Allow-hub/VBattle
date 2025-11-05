using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace TechC.VBattle.Core.Managers
{
    /// <summary>
    /// ManagerSceneを自動的に読み込むためのクラス
    /// </summary>
    public class ManagerSceneAutoLoader : MonoBehaviour
    {
        private const string managerSceneName = "ManagerScene";

        // ゲーム開始時(シーン読み込み前)に実行される
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadManagerScene()
        {
            // ManagerSceneが有効でない時(まだ読み込んでいない時)だけ追加ロードするように
            if (!SceneManager.GetSceneByName(managerSceneName).IsValid())
            {
                SceneManager.LoadScene(managerSceneName, LoadSceneMode.Additive);
                // コルーチンでシーンをアンロードする
                GameObject obj = new GameObject("ManagerSceneAutoLoader");
                obj.AddComponent<ManagerSceneAutoLoader>().StartCoroutine(UnloadManagerSceneAfterDelay());
            }
        }

        private static IEnumerator UnloadManagerSceneAfterDelay()
        {
            yield return null;

            // ManagerSceneをアンロード
            //string managerSceneName = "GameController";
            SceneManager.UnloadSceneAsync(managerSceneName);
        }
    }
}

