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
        [SerializeField] private Image p1DisplayImage;
        [SerializeField] private Image p2DisplayImage;
        [SerializeField] private Image p1NameImage;
        [SerializeField] private Image p2NameImage;
        [SerializeField] private SelectPickAnim p1SelectPickAnim;
        [SerializeField] private SelectPickAnim p2SelectPickAnim;
        [SerializeField] private Sprite p1CharaName;
        [SerializeField] private Sprite p2CharaName;

        [SerializeField] private Sprite p1CharaSprite;       // このボタンで選べるキャラのサムネ
        [SerializeField] private Sprite p2CharaSprite;       // このボタンで選べるキャラのサムネ
        [SerializeField] private CharacterData pickCharaData; // このボタンで選べるキャラ

        [Header("爆散用マテリアル")]
        [SerializeField] private Material explodeMaterial;
        private void OnValidate()
        {
#if UNITY_EDITOR
            if (p1DisplayImage == null)
                p1DisplayImage = GameObject.Find("p1DisplayImage")?.GetComponent<Image>();
            if (p1CharaName == null)
                p1NameImage = GameObject.Find("p1CharaName")?.GetComponent<Image>();

            if (p2DisplayImage == null)
                p2DisplayImage = GameObject.Find("p2DisplayImage")?.GetComponent<Image>();
            if (p2CharaName == null)
                p1NameImage = GameObject.Find("p2CharaName")?.GetComponent<Image>();
#endif
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (SelectUIManager.I == null || pickCharaData == null)
            {
                CustomLogger.Error("SelectUIManager or pickCharaData is null in OnPointerEnter!");
                return;
            }
            
            var (device, deviceName) = ResolveDevice(eventData);
            
            // デバイスからPlayerIdを判定
            int playerId = GetPlayerIdFromDevice(device);
            if (playerId == 0) return; // 無効なデバイス
            
            // ホバーイベント発行
            SelectUIManager.I.EventBus.Publish(new CharacterHoveredEvent
            {
                PlayerId = playerId,
                Character = pickCharaData,
                Device = device
            });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            var (device, deviceName) = ResolveDevice(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (SelectUIManager.I == null || pickCharaData == null)
            {
                CustomLogger.Error("SelectUIManager or pickCharaData is null in OnPointerClick!");
                return;
            }
            
            var (device, deviceName) = ResolveDevice(eventData);
            if (device == null) return;

            // デバイスからPlayerIdを判定
            int playerId = GetPlayerIdFromDevice(device);
            if (playerId == 0) return; // 無効なデバイス
            
            // ★重要：NPC判定を含めた選択確定イベント発行
            bool isNpc = SelectUIManager.I.GetIsNpc();
            InputDevice targetDevice = device;
            int targetId = playerId;
            
            // 1Pが確定済みで、2PがNPCの場合、2Pのキャラを選択できる
            if (SelectUIManager.I.CheckPicked(1) && isNpc && playerId == 1)
            {
                targetId = 2;
                targetDevice = null; // NPC
            }
            
            if (SelectUIManager.I.CheckPicked(targetId)) return;
            
            SelectUIManager.I.EventBus.Publish(new SelectionConfirmedEvent
            {
                PlayerId = targetId,
                Character = pickCharaData,
                Device = targetDevice,
                IsNpc = targetDevice == null
            });
            
            // UIアニメーションは残す
            Image target = (targetId == 1) ? p1DisplayImage : p2DisplayImage;
            if (target != null && explodeMaterial != null)
                StartCoroutine(PlayExplodeAnimation(target, targetId));
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
        
        /// <summary>
        /// デバイスからプレイヤーIDを判定（SelectUIManagerに委譲）
        /// </summary>
        private int GetPlayerIdFromDevice(InputDevice device)
        {
            if (SelectUIManager.I == null) return 0;
            return SelectUIManager.I.GetPlayerIdFromDevice(device);
        }

        private IEnumerator PlayExplodeAnimation(Image target, int id)
        {
            var originalMat = target.material;
            var instMat = new Material(explodeMaterial);
            target.material = instMat;

            float time = 0f;
            float duration = 1.2f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float progress = Mathf.Clamp01(time / duration);
                instMat.SetFloat("_Progress", progress);
                yield return null;
            }

            target.enabled = false;
            target.material = originalMat;
        }

    }
}