using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.Core.Managers
{
    
    /// <summary>
    /// シーン間でデータを受け渡すための永続化クラス
    /// ゲームロジックは持たず、データの保持・提供のみを行う
    /// </summary>
    public class GameDataBridge : Singleton<GameDataBridge>
    {
        [SerializeField] private int targetFrameRate = 144;
        [SerializeField] private bool isHighPerformanceMode = true;
        [SerializeField] private bool canConnectWifi = true;

        protected override bool UseDontDestroyOnLoad => true;
        protected override void Init()
        {
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;
        }

    }
}