using TechC.VBattle.Core.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TechC.VBattle.InGame.Character;
using TechC.VBattle.InGame.Events;

namespace TechC.VBattle.InGame.UI
{
    /// <summary>
    /// InGameのUIを制御するクラス
    /// </summary>
    public class InGameUIController : Singleton<InGameUIController>
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private CanvasGroup resultCanvasGroup;
        [SerializeField] private Button resultButton;
        [SerializeField] private Image p1IconImage;
        [SerializeField] private Image p2IconImage;
        [SerializeField] private Sprite ameIcon;
        [SerializeField] private Sprite teramiIcon;
        [SerializeField] private GameObject win1pImage;
        [SerializeField] private GameObject win2pImage;

        private float lastTime = 0;

        protected override bool UseDontDestroyOnLoad => false;
        public override void Init()
        {
            base.Init();
            timerText.text = $"{InGameManager.I.RemainingBattleTime:F0}/{InGameManager.I.BattleTimeLimit}";
            lastTime = InGameManager.I.RemainingBattleTime;
            SetResultCanvasActive(false);
        }

        private void Start()
        {
            if (resultButton == null) return;
            resultButton.onClick.AddListener(() =>
            {
                SceneLoader.I.LoadTitleSceneAsync().Forget();
            });
            InGameManager.I.BattleBus.Subscribe<PlayerOnDeathEvent>(e =>
            {
                if (e.PlayerIndex == 1)
                    win2pImage.SetActive(true);
                else if (e.PlayerIndex == 2)
                    win1pImage.SetActive(true);
            });
        }

        private void Update()
        {
            if (InGameManager.I == null) return;
            if (lastTime != InGameManager.I.RemainingBattleTime)
            {
                lastTime = InGameManager.I.RemainingBattleTime;
                timerText.text = $"{InGameManager.I.RemainingBattleTime:F0}/{InGameManager.I.BattleTimeLimit}";
            }
        }

        public void SetResultCanvasActive(bool isActive)
        {
            if (isActive)
            {
                if (GameDataBridge.I.Player_1Setup.SelectedCharacter.CharacterName == CharaName.Ame)
                    p1IconImage.sprite = ameIcon;
                else
                    p1IconImage.sprite = teramiIcon;
                if (GameDataBridge.I.Player_2Setup.SelectedCharacter.CharacterName == CharaName.Ame)
                    p2IconImage.sprite = ameIcon;
                else
                    p2IconImage.sprite = teramiIcon;
            }
            if (resultCanvasGroup != null)
            {
                resultCanvasGroup.alpha = isActive ? 1f : 0f;
                resultCanvasGroup.interactable = isActive;
                resultCanvasGroup.blocksRaycasts = isActive;
            }
        }
    }
}