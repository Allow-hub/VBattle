using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Select.Core;
using TechC.VBattle.Select.Events;
using TechC.VBattle.InGame.Character;
using TechC.VBattle.Core;

namespace TechC.VBattle.Select.UI
{
    /// <summary>
    /// キャラクター選択ボタンのインタラクション処理
    /// </summary>
    public class CharaButton : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
    {
        private const float EXPLODE_DURATION = 1.2f;
        private const string PROGRESS_SHADER_PROPERTY = "_Progress";

        [SerializeField] private Image p1DisplayImage;
        [SerializeField] private Image p2DisplayImage;
        [SerializeField] private Sprite p1CharaSprite;
        [SerializeField] private Sprite p2CharaSprite;
        [SerializeField] private Sprite p1CharaNameSprite;
        [SerializeField] private Sprite p2CharaNameSprite;
        [SerializeField] private Image p1CharaNameImage;
        [SerializeField] private Image p2CharaNameImage;


        [Header("1P / 2P / 両方の場合のアイコン")]
        [SerializeField] private Sprite p1SelectedIcon;
        [SerializeField] private Sprite p2SelectedIcon;
        [SerializeField] private Sprite bothSelectedIcon;


        [Header("キャラのデータ")]
        [SerializeField] private CharacterData pickCharaData;

        [Header("アイコンの後ろのSpriteの表示 / 非表示")]
        [SerializeField] private Image iconBackImage;

        [Header("爆散用マテリアル")]
        [SerializeField] private Material explodeMaterial;

        private Image selectionIconImage;
        private Sprite originalIconSprite;

        private void Start()
        {
            selectionIconImage = GetComponent<Image>();

            if (selectionIconImage != null)
                originalIconSprite = selectionIconImage.sprite;

            if (SelectUIManager.I != null)
            {
                SelectUIManager.I.EventBus.Subscribe<SelectionResetEvent>(OnSelectionReset);
                SelectUIManager.I.EventBus.Subscribe<SelectionUpdatedEvent>(OnSelectionUpdated);
            }
        }

        private void OnDestroy()
        {
            if (SelectUIManager.I != null)
            {
                SelectUIManager.I.EventBus.Unsubscribe<SelectionResetEvent>(OnSelectionReset);
                SelectUIManager.I.EventBus.Unsubscribe<SelectionUpdatedEvent>(OnSelectionUpdated);
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (p1DisplayImage == null)
                p1DisplayImage = GameObject.Find("p1DisplayImage")?.GetComponent<Image>();

            if (p2DisplayImage == null)
                p2DisplayImage = GameObject.Find("p2DisplayImage")?.GetComponent<Image>();
#endif
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (SelectUIManager.I == null || pickCharaData == null) return;

            // 両者選択済みの場合はホバー処理を無効化
            if (SelectUIManager.I.CheckPicked(PlayerConstants.PLAYER_1_ID) && SelectUIManager.I.CheckPicked(PlayerConstants.PLAYER_2_ID))
                return;

            var device = ResolveDevice(eventData);
            int playerId = GetPlayerIdFromDevice(device);

            if (!SelectUIManager.I.CheckPicked(PlayerConstants.PLAYER_1_ID))
                playerId = PlayerConstants.PLAYER_1_ID;
            else if (!SelectUIManager.I.CheckPicked(PlayerConstants.PLAYER_2_ID))
                playerId = PlayerConstants.PLAYER_2_ID;
            else if (playerId == 0)
                playerId = PlayerConstants.PLAYER_1_ID;

            Sprite targetSprite;
            Image targetNameImage;
            Sprite targetNameSprite;

            if (playerId == PlayerConstants.PLAYER_2_ID && p2CharaSprite != null)
            {
                targetSprite = p2CharaSprite;
                targetNameImage = p2CharaNameImage;
                targetNameSprite = p2CharaNameSprite;
            }
            else
            {
                targetSprite = p1CharaSprite;
                targetNameImage = p1CharaNameImage;
                targetNameSprite = p1CharaNameSprite;
            }

            if (targetNameImage != null && targetNameSprite != null)
                targetNameImage.sprite = targetNameSprite;

            if (iconBackImage != null)
                iconBackImage.enabled = true;

            SelectUIManager.I.EventBus.Publish(new CharacterHoveredEvent
            {
                PlayerId = playerId,
                Character = pickCharaData,
                Device = device,
                CharacterSprite = targetSprite
            });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (iconBackImage != null)
                iconBackImage.enabled = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (SelectUIManager.I == null || pickCharaData == null)
            {
                CustomLogger.Error("SelectUIManager or pickCharaData is null");
                return;
            }

            var device = ResolveDevice(eventData);
            if (device == null) return;

            int playerId = GetPlayerIdFromDevice(device);

            if (playerId == 0)
                playerId = !SelectUIManager.I.CheckPicked(PlayerConstants.PLAYER_1_ID) ? PlayerConstants.PLAYER_1_ID : PlayerConstants.PLAYER_2_ID;

            bool isNpc = SelectUIManager.I.GetIsNpc();
            InputDevice targetDevice = device;
            int targetId = playerId;

            // NPCが設定されている場合の処理
            if (isNpc)
            {
                // 1Pが既に選択済みの場合、2PをNPCとして設定
                if (SelectUIManager.I.CheckPicked(PlayerConstants.PLAYER_1_ID))
                {
                    targetId = PlayerConstants.PLAYER_2_ID;
                    targetDevice = null;  // NPCなのでデバイスはnull
                }
            }

            if (SelectUIManager.I.CheckPicked(targetId)) return;

            // 名前画像の更新（OnPointerEnterが呼ばれない場合に対応）
            Image targetNameImage = targetId == PlayerConstants.PLAYER_1_ID ? p1CharaNameImage : p2CharaNameImage;
            Sprite targetNameSprite = targetId == PlayerConstants.PLAYER_1_ID ? p1CharaNameSprite : p2CharaNameSprite;
            if (targetNameImage != null && targetNameSprite != null)
                targetNameImage.sprite = targetNameSprite;

            SelectUIManager.I.EventBus.Publish(new SelectionConfirmedEvent
            {
                PlayerId = targetId,
                Character = pickCharaData,
                Device = targetDevice,
                IsNpc = targetDevice == null
            });

            Image target = (targetId == PlayerConstants.PLAYER_1_ID) ? p1DisplayImage : p2DisplayImage;
            if (target != null && explodeMaterial != null)
                StartCoroutine(PlayExplodeAnimation(target));
        }

        private void UpdateSelectionIcon(CharacterData player1Character, CharacterData player2Character)
        {
            if (selectionIconImage == null) return;

            bool isSelectedByP1 = player1Character != null &&
                                  player1Character.CharacterName == pickCharaData.CharacterName;
            bool isSelectedByP2 = player2Character != null &&
                                  player2Character.CharacterName == pickCharaData.CharacterName;

            Sprite targetSprite = null;

            if (isSelectedByP1 && isSelectedByP2 && bothSelectedIcon != null)
                targetSprite = bothSelectedIcon;
            else if (isSelectedByP1 && p1SelectedIcon != null)
                targetSprite = p1SelectedIcon;
            else if (isSelectedByP2 && p2SelectedIcon != null)
                targetSprite = p2SelectedIcon;

            if (targetSprite != null)
                StartCoroutine(SelectUIAnimationUtility.FadeInWithScale(selectionIconImage, targetSprite));
        }

        private void OnSelectionUpdated(SelectionUpdatedEvent e)
        {
            UpdateSelectionIcon(e.Player1SelectedCharacter, e.Player2SelectedCharacter);
        }

        private void OnSelectionReset(SelectionResetEvent e)
        {
            if (selectionIconImage != null)
                selectionIconImage.sprite = originalIconSprite;
        }

        private InputDevice ResolveDevice(PointerEventData eventData)
        {
            if (eventData is UnityEngine.InputSystem.UI.ExtendedPointerEventData extended)
            {
                var device = extended.device;
                if (device is Mouse)
                    return Keyboard.current;
                if (device != null)
                    return device;
            }
            return null;
        }

        private int GetPlayerIdFromDevice(InputDevice device)
        {
            if (SelectUIManager.I == null) return 0;
            return SelectUIManager.I.GetPlayerIdFromDevice(device);
        }

        private IEnumerator PlayExplodeAnimation(Image target)
        {
            var originalMat = target.material;
            var instMat = new Material(explodeMaterial);
            target.material = instMat;

            float time = 0f;
            while (time < EXPLODE_DURATION)
            {
                time += Time.deltaTime;
                float progress = Mathf.Clamp01(time / EXPLODE_DURATION);
                instMat.SetFloat(PROGRESS_SHADER_PROPERTY, progress);
                yield return null;
            }

            target.enabled = false;
            target.material = originalMat;
        }
    }
}