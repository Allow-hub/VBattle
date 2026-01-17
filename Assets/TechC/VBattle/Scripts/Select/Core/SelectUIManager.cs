using TechC.VBattle.Audio;
using TechC.VBattle.Core.Extensions;
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
        public struct CharacterPick
        {
            public int playerId;
            public CharacterData characterData;
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
        [SerializeField] private CharacterData npcAmeData;
        [SerializeField] private CharacterData npcTeramiData;

        // ==============================
        // 公開プロパティ / コールバック
        // ==============================
        public System.Action OnStartGamePicked;
        public bool[] HasPicked => hasPicked;
        public CharacterPick[] CurrentPicks => currentPicks;
        public SelectEventBus EventBus => eventBus;

        // ==============================
        // 内部状態管理
        // ==============================
        private bool[] hasPicked = new bool[2];
        private CharacterPick[] currentPicks = new CharacterPick[2];
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
            currentPicks[0].playerId = 0;
            currentPicks[1].playerId = 1;
            
            // イベント購読
            eventBus.Subscribe<DeviceAssignedEvent>(OnDeviceAssigned);
            eventBus.Subscribe<CharacterHoveredEvent>(OnCharacterHovered);
            eventBus.Subscribe<SelectionConfirmedEvent>(OnSelectionConfirmed);
            eventBus.Subscribe<SelectionResetEvent>(OnSelectionReset);
        }
        
        private void OnDestroy()
        {
            // イベント購読解除
            eventBus.Unsubscribe<DeviceAssignedEvent>(OnDeviceAssigned);
            eventBus.Unsubscribe<CharacterHoveredEvent>(OnCharacterHovered);
            eventBus.Unsubscribe<SelectionConfirmedEvent>(OnSelectionConfirmed);
            eventBus.Unsubscribe<SelectionResetEvent>(OnSelectionReset);
            eventBus.Clear();
        }

        public bool GetIsNpc() => iconController_2p.GetCurrentDevice() == null;
        public bool CheckPicked(int id) => hasPicked[--id];
        
        /// <summary>
        /// デバイスからプレイヤーIDを判定（旧SetCharacterPickのロジックを流用）
        /// </summary>
        public int GetPlayerIdFromDevice(InputDevice device)
        {
            // 1Pがこのデバイスを持っている場合
            if (iconController_1p.GetCurrentDevice() == device)
                return 1;
            
            // 2Pがこのデバイスを持っている場合
            if (iconController_2p.GetCurrentDevice() == device)
                return 2;
            
            // 特別処理: 2PがNPCなら1Pのデバイスで2Pのキャラを選べる
            if (iconController_2p.GetCurrentDevice() == null && iconController_1p.GetCurrentDevice() == device)
            {
                if (CheckPicked(1))
                    return 2; // 1P確定済みなら2Pのキャラを選択
            }
            
            return 0; // どこにも割り当てできない
        }

        private void StartGame()
        {
            // AudioManager.I.PlaySE(SEID.ButtonClick);
            OnStartGamePicked?.Invoke();
        }

        private void ResetSelect()
        {
            eventBus.Publish(new SelectionResetEvent());
        }
        
        // ==============================
        // イベントハンドラー
        // ==============================
        
        private void OnDeviceAssigned(DeviceAssignedEvent e)
        {
            int index = e.PlayerId - 1;
            currentPicks[index].inputDevice = e.Device;
            CustomLogger.Info($"Player {e.PlayerId} device assigned: {e.Device?.displayName ?? "NPC"}");
        }
        
        private void OnCharacterHovered(CharacterHoveredEvent e)
        {
            // ホバー時のサムネイル更新処理は既存のChangePickThumbnailロジックを参照
            // ここではデータ更新のみ行い、UI更新は別途実装
        }
        
        private void OnSelectionConfirmed(SelectionConfirmedEvent e)
        {
            int index = e.PlayerId - 1;
            
            // ★NPC用のキャラクター変換処理
            CharacterData finalCharacter = e.Character;
            if (e.IsNpc)
            {
                if (e.Character.name.Contains("Ame"))
                    finalCharacter = npcAmeData;
                else if (e.Character.name.Contains("Terami"))
                    finalCharacter = npcTeramiData;
            }
            
            // ★重要：構造体全体を作り直して代入（部分更新を避ける）
            currentPicks[index] = new CharacterPick
            {
                playerId = index,
                characterData = finalCharacter,
                inputDevice = e.Device
            };
            
            hasPicked[index] = true;
            
            // 爆散アニメーション＋立ち絵表示
            var pickAnim = e.PlayerId == 1 ? selectPickAnim_1p : selectPickAnim_2p;
            pickAnim?.PlayAnim(finalCharacter.CharaPrefab);
            
            // 両プレイヤー準備完了チェック
            if (hasPicked[0] && hasPicked[1])
            {
                _ = DelayUtility.StartDelayedActionAsync(startDelay, () =>
                {
                    if (startObj != null)
                    {
                        startObj.SetActive(true);
                        eventBus.Publish(new BothPlayersReadyEvent());
                    }
                    else
                        CustomLogger.Error("startObjが設定されていません");
                });
            }
        }
        
        private void OnSelectionReset(SelectionResetEvent e)
        {
            startObj.SetActive(false);
            hasPicked[0] = false;
            hasPicked[1] = false;
            currentPicks[0].characterData = null;
            currentPicks[1].characterData = null;
            iconController_1p.InitIcon();
            iconController_2p.InitIcon();
            selectPickAnim_1p.ResetAnim();
            selectPickAnim_2p.ResetAnim();
            p1DisplayImage.enabled = true;
            p2DisplayImage.enabled = true;
            
            CustomLogger.Info("Selection reset");
        }
    }
}