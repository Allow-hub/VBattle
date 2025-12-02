using TechC.VBattle.InGame.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TechC.VBattle.InGame.Character;

namespace TechC.VBattle.InGame.UI
{
    /// <summary>
    /// プレイヤーのUIを制御するクラス
    /// </summary>
    public class PlayerUIController : MonoBehaviour
    {
        [Range(1, 2)]
        [SerializeField] private int targetPlayerIndex;
        
        // HP UI要素
        [Header("HP Display")]
        [SerializeField] private Slider hpSlider;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Image hpFillImage;
        [SerializeField] private Image backImage;
        
        [Header("Character Icons")]
        [SerializeField] private Image ameIconImage;
        [SerializeField] private Image teramiIconImage;
        
        [Header("HP Settings")]
        [SerializeField] private float smoothFillSpeed = 5f;
        [SerializeField] private float lowHpThreshold = 0.3f;
        [SerializeField] private Color normalHpColor = Color.green;
        [SerializeField] private Color lowHpColor = Color.red;
        [SerializeField] private Color color1p;
        [SerializeField] private Color color2p;

        private float targetHpPercentage = 1f;

        private void Start()
        {
            InGameManager.I.BattleBus.Subscribe<AttackResultEvent>(OnAttackResult);
            InitUI();
        }

        private void Update()
        {
            // スムーズなHPゲージアニメーション
            if (hpSlider != null && hpSlider.value != targetHpPercentage)
            {
                hpSlider.value = Mathf.Lerp(
                    hpSlider.value,
                    targetHpPercentage,
                    Time.deltaTime * smoothFillSpeed);
            }
        }

        private void InitUI()
        {
            // fillImageの取得
            if (hpFillImage == null && hpSlider != null)
                hpFillImage = hpSlider.fillRect.GetComponent<Image>();
            
            if (hpFillImage != null)
                hpFillImage.color = normalHpColor;
            
            // TODO: キャラクタータイプを取得して設定
            // SetCharacterIcon(characterType);
            SetBackgroundColor(targetPlayerIndex);
        }

        /// <summary>
        /// 攻撃結果イベントの処理
        /// </summary>
        private void OnAttackResult(AttackResultEvent e)
        {
            if (e.target?.PlayerIndex != targetPlayerIndex) return;
            if (!e.isHit) return;

            UpdateHP(e.target.CurrentHP, e.target.Data.MaxHP);
        }

        /// <summary>
        /// HP表示の更新
        /// </summary>
        private void UpdateHP(int currentHp, int maxHp)
        {
            float percentage = (float)currentHp / maxHp;
            targetHpPercentage = percentage;
            
            // テキスト更新（即座に反映）
            if (hpText != null)
            {
                hpText.text = $"{currentHp}/{maxHp}";
            }
            
            // 色変更
            if (hpFillImage != null)
            {
                hpFillImage.color = percentage <= lowHpThreshold ? lowHpColor : normalHpColor;
            }
        }

        private void SetCharacterIcon(CharaName charaName)
        {
            ameIconImage.gameObject.SetActive(false);
            teramiIconImage.gameObject.SetActive(false);

            switch (charaName)
            {
                case CharaName.Ame:
                    ameIconImage.gameObject.SetActive(true);
                    break;
                case CharaName.Terami:
                    teramiIconImage.gameObject.SetActive(true);
                    break;
            }
        }

        /// <summary>
        /// 背景色の設定
        /// </summary>
        /// <param name="playerId"></param>
        private void SetBackgroundColor(int playerId)
        {
            if (backImage != null)
                backImage.color = playerId == 1 ? color1p : color2p;
        }

        private void OnDestroy()
        {
            InGameManager.I?.BattleBus?.Unsubscribe<AttackResultEvent>(OnAttackResult);
        }
    }
}