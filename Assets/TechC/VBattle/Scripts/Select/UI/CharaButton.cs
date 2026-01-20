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
                CustomLogger.Error("SelectUIManager or pickCharaData is null");
                return;
            }
            
            var (device, deviceName) = ResolveDevice(eventData);
            int playerId = GetPlayerIdFromDevice(device);
            
            // PlayerIdが0の場合、まだ選択していない方のプレイヤーを対象にする
            if (playerId == 0)
            {
                playerId = !SelectUIManager.I.CheckPicked(1) ? 1 : 2;
            }
            
            // プレイヤーIDに対応するスプライトを選択（2Pスプライトがない場合は1Pを使用）
            Sprite targetSprite = (playerId == 1) ? p1CharaSprite : (p2CharaSprite != null ? p2CharaSprite : p1CharaSprite);
            
            // ホバーイベント発行
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
            var (device, deviceName) = ResolveDevice(eventData);
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
            {
                playerId = !SelectUIManager.I.CheckPicked(PLAYER_1_ID) ? PLAYER_1_ID : PLAYER_2_ID;
            }
            
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
            
            Image target = (targetId == PLAYER_1_ID) ? p1DisplayImage : p2DisplayImage;
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

            while (time < EXPLODE_DURATION)
            {
                time += Time.deltaTime;
                float progress = Mathf.Clamp01(time / EXPLODE_DURATION);
                instMat.SetFloat("_Progress", progress);
                yield return null;
            }

            target.enabled = false;
            target.material = originalMat;
        }

    }
}