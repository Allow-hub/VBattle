using TMPro;
using UnityEngine;
// using TechC.CommentSystem;

namespace TechC.VBattle.InGame.Gimmick.Tab
{
    public class NormalTab : BaseTab
    {
        // [SerializeField] private CommentDisplay commentDisplay;
        [SerializeField] private TextMeshProUGUI durationText;
        [SerializeField] private float addSpeed = 1.5f;
        [SerializeField] private float delayDuration = 3f;

        private float originalSpeed;

        protected override void Awake()
        {
            base.Awake();
            tabType = TabType.Normal;
        }

        [ContextMenu("Show")]
        public override void Show()
        {
            base.Show();
            Excute();
        }

        public override void Hide()
        {
            base.Hide();
        }

        public override void Excute()
        {
            base.Excute();
            durationText.text = delayDuration + "秒間";

            // originalSpeed = commentDisplay.GetCurrentSpeed();
            // // スピードを一時的に上げる
            // commentDisplay.AddSpeed(addSpeed);
            // // 速度をもとに戻す
            // DelayUtility.StartDelayedAction(this, delayDuration, () =>
            // {
            //     commentDisplay.SetSpeed(originalSpeed);
            // });
        }
    }
}
