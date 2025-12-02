using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Select.Core;

namespace TechC.VBattle.Select.UI
{
    /// <summary>
    /// キャラピックのイベントを送受信
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
        [SerializeField] private GameObject pickCharaPrefab; // このボタンで選べるキャラ

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
            if (SelectUIManager.I == null)
            {
                CustomLogger.Error("SelectUIManager.I is null in OnPointerEnter!");
                return;
            }
            
            if (pickCharaPrefab == null)
            {
                CustomLogger.Error("pickCharaPrefab is null in OnPointerEnter!");
                return;
            }
            
            var (device, deviceName) = ResolveDevice(eventData);
            int id = SelectUIManager.I.SetCharacterPick(device, pickCharaPrefab);
            ChangePickThumbnail(id);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            var (device, deviceName) = ResolveDevice(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (SelectUIManager.I == null)
            {
                CustomLogger.Error("SelectUIManager.I is null in OnPointerClick!");
                return;
            }
            
            if (pickCharaPrefab == null)
            {
                CustomLogger.Error("pickCharaPrefab is null in OnPointerClick!");
                return;
            }
            
            var (device, deviceName) = ResolveDevice(eventData);

            if (device != null)
            {
                int id = SelectUIManager.I.SetCharacterPick(device, pickCharaPrefab);
                DicidePick(id);
            }
        }

        private (InputDevice, string) ResolveDevice(PointerEventData eventData)
        {
            if (eventData is UnityEngine.InputSystem.UI.ExtendedPointerEventData extended)
            {
                var device = extended.device;
                if (device is Mouse)
                {
                    return (Keyboard.current, "Keyboard");
                }
                else if (device != null)
                {
                    return (device, device.displayName);
                }
                else
                {
                    return (null, "不明");
                }
            }
            return (null, "旧InputSystem");
        }

        private void ChangePickThumbnail(int id)
        {
            if (id == 0) return;
            
            if (SelectUIManager.I == null)
            {
                CustomLogger.Error("SelectUIManager.I is null in ChangePickThumbnail!");
                return;
            }

            if (id == 1)
            {
                if (SelectUIManager.I.CheckPicked(id))//1pが選択済みで2pがNPCのとき1pが2pのキャラを選択できるように
                {
                    if (!SelectUIManager.I.GetIsNpc()) return;
                    p2DisplayImage.sprite = p2CharaSprite;
                    if (!SelectUIManager.I.CheckPicked(++id))
                        p2NameImage.sprite = p2CharaName;
                }
                else
                {
                    p1DisplayImage.sprite = p1CharaSprite;
                    if (!SelectUIManager.I.CheckPicked(id))
                        p1NameImage.sprite = p1CharaName;
                }
            }
            else
            {
                p2DisplayImage.sprite = p2CharaSprite;
                if (!SelectUIManager.I.CheckPicked(id))
                    p2NameImage.sprite = p2CharaName;
            }
        }

        private void DicidePick(int id)
        {
            if (id == 0) return;
            
            if (SelectUIManager.I == null)
            {
                CustomLogger.Error("SelectUIManager.I is null in DicidePick!");
                return;
            }
            if (SelectUIManager.I.CheckPicked(id))//1pが選択済みで2pがNPCのとき1pが2pのキャラを選択できるように
            {
                if (!SelectUIManager.I.GetIsNpc()) return;
                id = 2;
                if (SelectUIManager.I.CheckPicked(id)) return;
                Image target = (id == 1) ? p1DisplayImage : p2DisplayImage;
                if (target == null || explodeMaterial == null) return;

                // 爆散アニメーションを開始
                StartCoroutine(PlayExplodeAnimation(target, id));
            }
            else
            {
                Image target = (id == 1) ? p1DisplayImage : p2DisplayImage;
                if (target == null || explodeMaterial == null) return;

                // 爆散アニメーションを開始
                StartCoroutine(PlayExplodeAnimation(target, id));
            }
        }

        private IEnumerator PlayExplodeAnimation(Image target, int id)
        {
            var originalMat = target.material;
            var instMat = new Material(explodeMaterial);
            target.material = instMat;

            float time = 0f;
            float duration = 1.2f;
            if (id == 1 && p1SelectPickAnim != null)
            {
                p1SelectPickAnim.PlayAnim(pickCharaPrefab);
            }
            else if (id == 2 && p2SelectPickAnim != null)
            {
                p2SelectPickAnim.PlayAnim(pickCharaPrefab);
            }
            
            if (SelectUIManager.I != null)
            {
                SelectUIManager.I.SetPicked(id, true);
            }

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