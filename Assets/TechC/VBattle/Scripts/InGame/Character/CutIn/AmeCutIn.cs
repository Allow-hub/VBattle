using System;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Window;
using UnityEngine;
using Windows.Win32.Foundation;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// アメのカットイン
    /// </summary>
    public class AmeCutIn : MonoBehaviour, ICutInSequence
    {
        [SerializeField] private Vector3 p1Pos;
        [SerializeField] private Vector3 p1Rot;
        [SerializeField] private Vector3 p2Pos;
        [SerializeField] private Vector3 p2Rot;

        public event Action OnFinished;

        private void OnEnable()
        {
            Play().Forget();
        }

        public async UniTask Play()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            //TODO:必殺技を打ったのがプレイヤー1かプレイヤー2かで、どちらのキャラを先に出すか変える
            var p1Obj = Instantiate(GameDataBridge.I.Player_1Setup.SelectedCharacter.CharaPrefab, p1Pos, Quaternion.Euler(p1Rot));
            var p2Obj = Instantiate(GameDataBridge.I.Player_2Setup.SelectedCharacter.CharaPrefab, p2Pos, Quaternion.Euler(p2Rot));

            WindowManager.I.ResetWindow(true);

            OnFinished?.Invoke();
        }
    }
}
