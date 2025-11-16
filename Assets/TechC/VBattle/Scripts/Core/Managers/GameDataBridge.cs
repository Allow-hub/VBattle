using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.Core.Managers
{

    /// <summary>
    /// シーン間でデータを受け渡すための永続化クラス
    /// ゲームロジックは持たず、データの保持・提供のみを行う
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public class GameDataBridge : Singleton<GameDataBridge>
    {
        [SerializeField] private bool isDebug;
        [SerializeField] private int targetFrameRate = 144;
        [SerializeField] private bool isHighPerformanceMode = true;
        [SerializeField] private bool canConnectWifi = true;

        public bool IsDebug => isDebug;

        protected override bool UseDontDestroyOnLoad => true;
        public override void Init()
        {
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;
        }

    }
}