using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TechC.Select
{
    [System.Serializable]
    public class IconData
    {
        public GameObject iconObject;
        public Sprite iconSprite;
        public InputDevice device; // null = NPC
    }

    /// <summary>
    /// InputDeviceのアイコン管理
    /// </summary>
    public class IconController : MonoBehaviour
    {
        [SerializeField] private bool isP1;
        [SerializeField] private GameObject iconPrefab;
        [SerializeField] private Sprite keyboardSprite;
        [SerializeField] private Sprite gamepadSprite;
        [SerializeField] private Sprite npcSprite;

        [Header("配置設定")]
        [SerializeField] private float radius = 100f;
        [SerializeField] private float angleStep = 20f;
        [SerializeField] private float animDuration = 0.5f;
        [SerializeField] private float animDelay = 0.2f;
        [SerializeField] private float clearDistance = 200f;

        private List<IconData> generatedIcons = new List<IconData>();
        private IconData currentIconData;

        private bool iconsActive = false;
        private bool isClearing = false;
        private float lastOpenedTime = -999f;
        private float autoCloseDelay = 1f;
        private Camera cam;
        private Image iconImage;

        private void Awake()
        {
            iconImage = GetComponent<Image>();
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClicked);

            var canvas = GetComponentInParent<Canvas>();
            cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            InitIcon();
        }

        private void Update()
        {
            if (iconsActive && !isClearing && generatedIcons.Count > 0)
            {
                if (Time.time - lastOpenedTime < autoCloseDelay) return;

                Vector2 centerScreenPos = RectTransformUtility.WorldToScreenPoint(cam, transform.position);
                float distance = Vector2.Distance(centerScreenPos, Input.mousePosition);

                if (distance > clearDistance)
                {
                    StartCoroutine(ClearIcons());
                }
            }
        }

        private void OnButtonClicked()
        {
            if (iconsActive && !isClearing)
            {
                StartCoroutine(ClearIcons());
            }
            else
            {
                GenerateIcons();
            }
        }

        private void GenerateIcons()
        {
            ClearImmediate();
            lastOpenedTime = Time.time;

            List<Sprite> sprites = new List<Sprite>();
            List<InputDevice> devices = new List<InputDevice>();

            foreach (var device in InputSystem.devices)
            {
                if (device is Keyboard)
                {
                    sprites.Add(keyboardSprite);
                    devices.Add(device);
                }
                else if (device is Gamepad)
                {
                    sprites.Add(gamepadSprite);
                    devices.Add(device);
                }
            }

            if (!isP1)
            {
                sprites.Add(npcSprite);
                devices.Add(null);
            }

            float startAngle = isP1 ? 45f : 135f;
            float direction = isP1 ? -1f : 1f;

            for (int i = 0; i < sprites.Count; i++)
            {
                float angle = startAngle + (i * angleStep * direction);
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad),
                    0f
                ) * radius;

                var iconObj = Instantiate(iconPrefab, transform);
                var rect = iconObj.GetComponent<RectTransform>();
                rect.anchoredPosition = Vector2.zero;

                var image = iconObj.GetComponent<Image>();
                if (image != null) image.sprite = sprites[i];

                var iconData = new IconData
                {
                    iconObject = iconObj,
                    iconSprite = sprites[i],
                    device = devices[i]
                };
                generatedIcons.Add(iconData);

                var button = iconObj.GetComponent<Button>();
                button.onClick.AddListener(() => ChildClickEvent(iconData));

                StartCoroutine(MoveToPosition(rect, offset, animDuration, i * animDelay));
            }

            iconsActive = true;
        }

        private IEnumerator ClearIcons()
        {
            isClearing = true;
            iconsActive = false;

            for (int i = 0; i < generatedIcons.Count; i++)
            {
                if (generatedIcons[i] == null) continue;

                var rect = generatedIcons[i].iconObject.GetComponent<RectTransform>();
                StartCoroutine(MoveToPosition(rect, Vector2.zero, animDuration, i * animDelay, true));
            }

            yield return new WaitForSeconds(animDuration + generatedIcons.Count * animDelay);
            ClearImmediate();

            isClearing = false;
        }

        private void ClearImmediate()
        {
            foreach (var icon in generatedIcons)
            {
                if (icon.iconObject != null)
                    Destroy(icon.iconObject);
            }
            generatedIcons.Clear();
        }

        private IEnumerator MoveToPosition(RectTransform rect, Vector3 target, float duration, float delay, bool destroyAfter = false)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            Vector3 start = rect.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);
                rect.anchoredPosition = Vector3.Lerp(start, target, t);
                yield return null;
            }

            rect.anchoredPosition = target;

            if (destroyAfter) Destroy(rect.gameObject);
        }

        private void ChildClickEvent(IconData clickedIconData)
        {
            if (clickedIconData != null)
            {
                iconImage.sprite = clickedIconData.iconSprite;
                currentIconData = clickedIconData; // 選択情報を更新
            }
            StartCoroutine(ClearIcons());
        }

        /// <summary>
        /// 現在選択中のデバイスを返す
        /// </summary>
        public InputDevice GetCurrentDevice() => currentIconData.device;

        /// <summary>
        /// 初期状態に戻す（外部から呼び出し可能）
        /// </summary>
        public void InitIcon()
        {
            if (iconImage == null)
                iconImage = GetComponent<Image>();

            if (isP1)
            {
                iconImage.sprite = keyboardSprite;
                currentIconData = new IconData { iconSprite = keyboardSprite, device = Keyboard.current };
            }
            else
            {
                bool existDevice = false;
                foreach (var device in InputSystem.devices)
                {
                    if (device is Keyboard) continue;
                    if (device is Gamepad)
                    {
                        iconImage.sprite = gamepadSprite;
                        currentIconData = new IconData { iconSprite = gamepadSprite, device = device };
                        existDevice = true;
                        break;
                    }
                }

                if (!existDevice)
                {
                    iconImage.sprite = npcSprite;
                    currentIconData = new IconData { iconSprite = npcSprite, device = null };
                }
            }
        }
    }
}