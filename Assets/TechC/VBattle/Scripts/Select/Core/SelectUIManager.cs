using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Util;
using TechC.VBattle.InGame.Character;
using TechC.VBattle.Select.UI;
using TechC.VBattle.Select.Events;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TechC.VBattle.Select.Core
{
    /// <summary>
    /// キャラクター選択のUI制御とプレイヤー入力処理を管理
    /// </summary>
    public class SelectUIManager : Singleton<SelectUIManager>
    {
        private const int PLAYER_COUNT = 2;
        private const int PLAYER_1_INDEX = 0;
        private const int PLAYER_2_INDEX = 1;
        private const int PLAYER_1_ID = 1;
        private const int PLAYER_2_ID = 2;
        private const int PLAYER_ID_UNKNOWN = 0;
        private const string CHARACTER_NAME_AME = "Ame";
        private const string CHARACTER_NAME_TERAMI = "Terami";


        public struct CharacterPick
        {
            public int playerId;
            public CharacterData characterData;
            public InputDevice inputDevice;
        }

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
        [SerializeField] private CharacterData npcAmeData;
        [SerializeField] private CharacterData npcTeramiData;

        public System.Action OnStartGamePicked;
        public bool[] HasPicked => hasPicked;
        public CharacterPick[] CurrentPicks => currentPicks;
        public SelectEventBus EventBus => eventBus;

        private bool[] hasPicked = new bool[PLAYER_COUNT];
        private CharacterPick[] currentPicks = new CharacterPick[PLAYER_COUNT];
        private SelectEventBus eventBus = new();
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
            currentPicks[PLAYER_1_INDEX].playerId = PLAYER_1_INDEX;
            currentPicks[PLAYER_2_INDEX].playerId = PLAYER_2_INDEX;
            
            // イベント購読
            eventBus.Subscribe<DeviceAssignedEvent>(OnDeviceAssigned);
            eventBus.Subscribe<SelectionConfirmedEvent>(OnSelectionConfirmed);
            eventBus.Subscribe<SelectionResetEvent>(OnSelectionReset);
            eventBus.Subscribe<CharacterHoveredEvent>(OnCharacterHovered);
        }
        
        private void OnDestroy()
        {
            eventBus.Unsubscribe<DeviceAssignedEvent>(OnDeviceAssigned);
            eventBus.Unsubscribe<SelectionConfirmedEvent>(OnSelectionConfirmed);
            eventBus.Unsubscribe<SelectionResetEvent>(OnSelectionReset);
            eventBus.Unsubscribe<CharacterHoveredEvent>(OnCharacterHovered);
            eventBus.Clear();
        }

        public bool GetIsNpc() => iconController_2p.GetCurrentDevice() == null;
        public bool CheckPicked(int id) => hasPicked[--id];
        
        /// <summary>
        /// デバイスからプレイヤーIDを判定
        /// </summary>
        public int GetPlayerIdFromDevice(InputDevice device)
        {
            if (iconController_1p.GetCurrentDevice() == device)
                return PLAYER_1_ID;
            
            if (iconController_2p.GetCurrentDevice() == device)
                return PLAYER_2_ID;
            
            if (iconController_2p.GetCurrentDevice() == null && iconController_1p.GetCurrentDevice() == device && CheckPicked(PLAYER_1_ID))
                return PLAYER_2_ID;
            
            return PLAYER_ID_UNKNOWN;
        }

        private void StartGame() => OnStartGamePicked?.Invoke();
        private void ResetSelect() => eventBus.Publish(new SelectionResetEvent());
        
        private void OnDeviceAssigned(DeviceAssignedEvent e)
        {
            int index = e.PlayerId - 1;
            currentPicks[index].inputDevice = e.Device;
        }
        
        
        private void OnSelectionConfirmed(SelectionConfirmedEvent e)
        {
            int index = e.PlayerId - 1;
            CharacterData finalCharacter = e.Character;

            if (e.IsNpc)
            {
                if (e.Character.name.Contains(CHARACTER_NAME_AME))
                    finalCharacter = npcAmeData;
                else if (e.Character.name.Contains(CHARACTER_NAME_TERAMI))
                    finalCharacter = npcTeramiData;
            }
            
            currentPicks[index] = new CharacterPick
            {
                playerId = index,
                characterData = finalCharacter,
                inputDevice = e.Device
            };
            
            hasPicked[index] = true;
            
            var pickAnim = e.PlayerId == PLAYER_1_ID ? selectPickAnim_1p : selectPickAnim_2p;
            pickAnim.PlayAnim(finalCharacter.CharaPrefab);
            
            // 状態更新完了を通知
            eventBus.Publish(new SelectionUpdatedEvent
            {
                Player1SelectedCharacter = hasPicked[PLAYER_1_INDEX] ? currentPicks[PLAYER_1_INDEX].characterData : null,
                Player2SelectedCharacter = hasPicked[PLAYER_2_INDEX] ? currentPicks[PLAYER_2_INDEX].characterData : null
            });
            
            if (hasPicked[PLAYER_1_INDEX] && hasPicked[PLAYER_2_INDEX])
            {
                _ = DelayUtility.StartDelayedActionAsync(startDelay, () =>
                {
                    if (startObj == null)
                    {
                        Debug.LogError("startObjが設定されていません");
                        return;
                    }
                    startObj.SetActive(true);
                });
            }
        }
        
        private void OnSelectionReset(SelectionResetEvent e)
        {
            startObj.SetActive(false);
            hasPicked[PLAYER_1_INDEX] = false;
            hasPicked[PLAYER_2_INDEX] = false;
            currentPicks[PLAYER_1_INDEX].characterData = null;
            currentPicks[PLAYER_2_INDEX].characterData = null;
            iconController_1p.InitIcon();
            iconController_2p.InitIcon();
            selectPickAnim_1p.ResetAnim();
            selectPickAnim_2p.ResetAnim();
            p1DisplayImage.enabled = true;
            p2DisplayImage.enabled = true;
        }
        
        private void OnCharacterHovered(CharacterHoveredEvent e)
        {
            Image targetImage = e.PlayerId == PLAYER_1_ID ? p1DisplayImage : p2DisplayImage;
            if (targetImage == null || e.CharacterSprite == null) return;
            targetImage.sprite = e.CharacterSprite;
        }
    }
}