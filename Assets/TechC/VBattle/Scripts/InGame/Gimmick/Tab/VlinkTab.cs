using UnityEngine;

namespace TechC.VBattle.InGame.Gimmick.Tab
{
    public class VlinkTab : BaseTab
    {
        void Start()
        {
            Show(); // 初期表示される場合
        }

        public override void Show()
        {
            base.Show();
            // 例：Vlink関連のUI表示処理
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            base.Hide();
            gameObject.SetActive(false);
        }

        public override void Excute()
        {
            base.Excute();
            Debug.Log("Vlinkタブの処理を実行");
            // 例：バーチャルリンクの詳細表示など
        }
    }
}
