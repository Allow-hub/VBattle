using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Util;
using TechC.VBattle.Select.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TechC.VBattle.Select.Core
{
    public class SelectUIManager : Singleton<SelectUIManager>
    {
        public struct CharacterPick
        {
            public int playerId;
            public GameObject characterObject;
            public InputDevice inputDevice;
        }

        // ==============================
        // Inspector設定用
        // ==============================
        [SerializeField] private float startDelay = 6f;
        [SerializeField] private GameObject startObj;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button startButton;
        [SerializeField] private IconController iconController_1p;
        [SerializeField] private IconController iconController_2p;
        [SerializeField] private SelectPickAnim selectPickAnim_1p;
        [SerializeField] private SelectPickAnim selectPickAnim_2p;
        [SerializeField] private Image p1DisplayImage;
        [SerializeField] private Image p2DisplayImage;
        [SerializeField] private GameObject npcAmePrefab;
        [SerializeField] private GameObject npcTeramiPrefab;

        // ==============================
        // 公開プロパティ / コールバック
        // ==============================
        public System.Action OnStartGamePicked;
        public bool[] HasPicked => hasPicked;
        public CharacterPick[] CurrentPicks => currentPicks;

        // ==============================
        // 内部状態管理
        // ==============================
        private bool[] hasPicked = new bool[2];
        private CharacterPick[] currentPicks = new CharacterPick[2];
        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
        }

        private void Start()
        {
            startButton.onClick.AddListener(StartGame);
            cancelButton.onClick.AddListener(ResetSelect);
            startObj.SetActive(false);
            currentPicks[0].playerId = 0;
            currentPicks[1].playerId = 1;
        }

        /// <summary>
        /// キャラ選択時そのデバイスが使用中であるかどうかで値を変える
        /// </summary>
        /// <param name="inputDevice">入力が加えられたデバイス</param>
        /// <param name="pickChara">ピックされたキャラ</param>
        /// <returns>1->1p,2->2p,0->無効なデバイス</returns>
        public int SetCharacterPick(InputDevice inputDevice, GameObject pickChara)
        {
            // --- 1Pがこのデバイスを持っている場合
            if (iconController_1p.GetCurrentDevice() == inputDevice)
            {
                currentPicks[0].characterObject = pickChara;
                currentPicks[0].inputDevice = inputDevice;
                return 1;
            }

            // --- 2Pがこのデバイスを持っている場合
            if (iconController_2p.GetCurrentDevice() == inputDevice)
            {
                currentPicks[1].characterObject = pickChara;
                currentPicks[1].inputDevice = inputDevice;
                return 2;
            }

            // --- 特別処理: 2PがNPCなら1Pのデバイスで2Pのキャラを選べる
            if (iconController_2p.GetCurrentDevice() == null)
            {
                // 1Pがもうキャラを決定済みか確認
                if (CheckPicked(1))
                {
                    if (pickChara.name.Contains("Ame"))
                        currentPicks[1].characterObject = npcAmePrefab;
                    else if (pickChara.name.Contains("Terami"))
                        currentPicks[1].characterObject = npcTeramiPrefab;
                    else
                        currentPicks[1].characterObject = pickChara;

                    currentPicks[1].inputDevice = null;
                    return 2;
                }
            }

            // --- どこにも割り当てできない場合は無効
            return 0;
        }

        public void SetPicked(int id, bool b)
        {
            id--;
            hasPicked[id] = b;
            if (hasPicked[0] && hasPicked[1])
            {
                // 遅延してstartObjを表示
                _ = DelayUtility.StartDelayedActionAsync(startDelay, () =>
                {
                    if (startObj != null)
                    {
                        startObj.SetActive(true);
                    }
                    else
                    {
                        CustomLogger.Error("startObjが設定されていません");
                    }
                });
                
                // StartWindow.I.ShowStartWindow(); // TODO: StartWindowを有効にする場合
            }
        }

        public bool GetIsNpc() => iconController_2p.GetCurrentDevice() == null;
        public bool CheckPicked(int id) => hasPicked[--id];

        private void StartGame()
        {
            // AudioManager.I.PlaySE(SEID.ButtonClick); // TODO：AudioManagerを入れたら解除する
            OnStartGamePicked?.Invoke();
        }

        private void ResetSelect()
        {
            startObj.SetActive(false);
            hasPicked[0] = false;
            hasPicked[1] = false;
            currentPicks[0].characterObject = null;
            currentPicks[1].characterObject = null;
            iconController_1p.InitIcon();
            iconController_2p.InitIcon();
            selectPickAnim_1p.ResetAnim();
            selectPickAnim_2p.ResetAnim();
            p1DisplayImage.enabled = true;
            p2DisplayImage.enabled = true;
        }
    }
}