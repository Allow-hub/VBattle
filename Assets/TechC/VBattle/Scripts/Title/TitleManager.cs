using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC.VBattle.Title
{
    public class TitleManager : MonoBehaviour
    {
        [SerializeField] GameObject MoviePlayer;
        [SerializeField] private float ChangeTime = 5.0f;

        private bool MoviePlaying = false;

        private InputAction pressAnyKeyAction =
            new InputAction(type: InputActionType.PassThrough, binding: "*/<Button>", interactions: "Press");

        private void OnEnable() => pressAnyKeyAction.Enable();
        private void OnDisable() => pressAnyKeyAction.Disable();

        void Start()
        {
            MoviePlayer.gameObject.SetActive(false);
            StartCoroutine(PlayMovieWithDelay(ChangeTime)); // ポーズ画面がないためコルーチンを使用
        }

        void Update()
        {
            if (pressAnyKeyAction.triggered)
            {
                if (MoviePlaying)
                {
                    StopMovie();
                }
                else
                {
                    // GameManager.I.ChangeSelectState(); // TODO:GameManagerがインポートできたらコメントアウトを消す
                }
            }
        }

        /// <summary>
        /// Coroutineで遅延処理を実現
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator PlayMovieWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayMovie();
        }

        /// <summary>
        /// 映像再生を開始
        /// </summary>
        private void PlayMovie()
        {
            // if (!MoviePlaying)
            // {
            //     MoviePlayer.gameObject.SetActive(true);
            //     MoviePlaying = true;
            // }
        }

        /// <summary>
        /// 映像再生を停止
        /// </summary>
        private void StopMovie()
        {
            MoviePlayer.gameObject.SetActive(false);
            MoviePlaying = false;
            StartCoroutine(PlayMovieWithDelay(ChangeTime)); // 次の再生をスケジュール
        }
    }
}