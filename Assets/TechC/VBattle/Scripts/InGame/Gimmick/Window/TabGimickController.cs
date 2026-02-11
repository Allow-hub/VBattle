using System;
using TechC.VBattle.InGame.Gimmick.Tab;
using UnityEngine;

namespace TechC.VBattle.InGame.Gimmick
{
    /// <summary>
    /// タブのギミック管理
    /// </summary>
    [Serializable]
    public class TabGimickController : IGimmick
    {
        [SerializeField] private GameObject normalTabObj;
        private NormalTab normalTab;
        [SerializeField] private Vector2 intervalRange;
        
        private float currentInterval;
        private int tabTypeLength;
        private TabType currnetTabType;
        private float timer;
        
        // タブ表示状態の管理
        private bool isTabShowing = false;
        private float tabShowTimer = 0f;
        
        public void OnEnter()
        {
            Lottery();
            timer = 0f;
            tabTypeLength = Enum.GetNames(typeof(TabType)).Length;
            normalTab = normalTabObj.GetComponent<NormalTab>();
            isTabShowing = false;
            tabShowTimer = 0f;
        }

        public void OnUpdate(float deltaTime)
        {
            // タブ表示中の処理
            if (isTabShowing)
            {
                tabShowTimer += deltaTime;
                
                // タブの表示時間が終了したら次のインターバル開始
                if (tabShowTimer >= normalTab.VisibleTime)
                {
                    isTabShowing = false;
                    tabShowTimer = 0f;
                    Lottery();  // 次回の抽選
                    timer = 0f;
                }
                return;  // タブ表示中は通常のタイマーを進めない
            }
            
            // 通常のインターバルカウント
            timer += deltaTime;
            if (timer >= currentInterval)
            {
                ExecuteTabEvent();
                timer = 0f;
            }
        }

        public void OnExit()
        {
            // 終了時にタブを非表示にする（必要なら）
            if (isTabShowing && normalTab != null)
            {
                normalTab.Hide();  // Hideメソッドがある場合
            }
            isTabShowing = false;
            tabShowTimer = 0f;
        }

        /// <summary>
        /// タブの抽選とインターバルの抽選
        /// </summary>
        private void Lottery()
        {
            currentInterval = UnityEngine.Random.Range(intervalRange.x, intervalRange.y);
            currnetTabType = (TabType)UnityEngine.Random.Range(0, tabTypeLength);
        }

        /// <summary>
        /// タブイベントの実行
        /// </summary>
        private void ExecuteTabEvent()
        {
            switch (currnetTabType)
            {
                case TabType.Normal:
                    normalTab.Show();
                    isTabShowing = true;
                    tabShowTimer = 0f;
                    break;
            }
        }
    }
}