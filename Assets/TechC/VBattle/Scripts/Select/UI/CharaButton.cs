using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Select.Core;
using TechC.VBattle.Select.Events;
using TechC.VBattle.InGame.Character;

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
        private const int PLAYER_1_ID = 1;
        private const int PLAYER_2_ID = 2;
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
        [SerializeField] private Image selectionIconImage;
        [SerializeField] private Sprite p1SelectedIcon;
        [SerializeField] private Sprite p2SelectedIcon;
        [SerializeField] private Sprite bothSelectedIcon;


        [Header("キャラのデータ")]
        [SerializeField] private CharacterData pickCharaData;

        [Header("アイコンの後ろのSpriteの表示 / 非表示")]
        [SerializeField] private Image iconBackImage;

        [Header("爆散用マテリアル")]
        [SerializeField] private Material explodeMaterial;

        private void Start()
        {
            if (SelectUIManager.I != null)
                SelectUIManager.I.EventBus.Subscribe<SelectionResetEvent>(OnSelectionReset);
        }

        private void OnDestroy()
        {
            if (SelectUIManager.I != null)
                SelectUIManager.I.EventBus.Unsubscribe<SelectionResetEvent>(OnSelectionReset);
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

            var (device, deviceName) = ResolveDevice(eventData);
            int playerId = GetPlayerIdFromDevice(device);

            if (!SelectUIManager.I.CheckPicked(PLAYER_1_ID))
                playerId = PLAYER_1_ID;
            else if (!SelectUIManager.I.CheckPicked(PLAYER_2_ID))
                playerId = PLAYER_2_ID;
            else if (playerId == 0)
                playerId = PLAYER_1_ID;

            Sprite targetSprite;
            Image targetNameImage;
            Sprite targetNameSprite;
            
            if (playerId == PLAYER_2_ID && p2CharaSprite != null)
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

            var (device, deviceName) = ResolveDevice(eventData);
            if (device == null) return;

            int playerId = GetPlayerIdFromDevice(device);

            // PlayerIdが0の場合、まだ選択していない方のプレイヤーを対象にする
            if (playerId == 0)
                playerId = !SelectUIManager.I.CheckPicked(PLAYER_1_ID) ? PLAYER_1_ID : PLAYER_2_ID;

            bool isNpc = SelectUIManager.I.GetIsNpc();
            InputDevice targetDevice = device;
            int targetId = playerId;

            if (SelectUIManager.I.CheckPicked(PLAYER_1_ID) && isNpc && playerId == PLAYER_1_ID)
            {
                targetId = PLAYER_2_ID;
                targetDevice = null;
            }

            if (SelectUIManager.I.CheckPicked(targetId)) return;

            SelectUIManager.I.EventBus.Publish(new SelectionConfirmedEvent
            {
                PlayerId = targetId,
                Character = pickCharaData,
                Device = targetDevice,
                IsNpc = targetDevice == null
            });

            UpdateSelectionIcon(targetId);

            Image target = (targetId == PLAYER_1_ID) ? p1DisplayImage : p2DisplayImage;
            if (target != null && explodeMaterial != null)
                StartCoroutine(PlayExplodeAnimation(target));
        }

        private void UpdateSelectionIcon(int playerId)
        {
            if (selectionIconImage == null) return;

            if (playerId == PLAYER_1_ID && p1SelectedIcon != null)
                selectionIconImage.sprite = p1SelectedIcon;
            else if (playerId == PLAYER_2_ID && p2SelectedIcon != null)
                selectionIconImage.sprite = p2SelectedIcon;
        }

        private void OnSelectionReset(SelectionResetEvent e)
        {
            if (selectionIconImage != null && bothSelectedIcon != null)
                selectionIconImage.sprite = bothSelectedIcon;
        }

        private (InputDevice, string) ResolveDevice(PointerEventData eventData)
        {
            if (eventData is UnityEngine.InputSystem.UI.ExtendedPointerEventData extended)
            {
                var device = extended.device;
                if (device is Mouse)
                    return (Keyboard.current, "Keyboard");
                else if (device != null)
                    return (device, device.displayName);
                else
                    return (null, "不明");
            }
            return (null, "旧InputSystem");
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