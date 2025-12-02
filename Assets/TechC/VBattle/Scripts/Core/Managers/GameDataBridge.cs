using TechC.VBattle.InGame.Character;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC.VBattle.Core.Managers
{
    /// <summary>
    /// シーン間でデータを受け渡すための永続化クラス
    /// ゲームロジックは持たず、データの保持・提供のみを行う
    /// </summary>
    public class GameDataBridge : Singleton<GameDataBridge>
    {
        [SerializeField] private int targetFrameRate = 144;
        [SerializeField] private bool isHighPerformanceMode = true;
        [SerializeField] private bool canConnectWifi = true;

        public bool IsPaused => isPaused;
        private bool isPaused = false;

        /// <summary>
        /// セレクト画面で設定される情報
        /// </summary>
        public class PlayerSetupData
        {
            public int PlayerIndex { get; set; }
            public InputDevice DeviceName { get; set; }
            public bool IsNPC { get; set; }
            public CharacterData SelectedCharacter { get; set; }
        }

        public PlayerSetupData Player_1Setup { get; private set; }
        public PlayerSetupData Player_2Setup { get; private set; }
        protected override bool UseDontDestroyOnLoad => true;
        public override void Init()
        {
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;
        }

        public void SetupPlayer(int playerIndex, PlayerSetupData data)
        {
            if (playerIndex == 0) Player_1Setup = data;
            else Player_2Setup = data;
        }
    }
}