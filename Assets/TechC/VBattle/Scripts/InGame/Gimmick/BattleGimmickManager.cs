using System.Collections.Generic;
using TechC.VBattle.Core.Managers;
using UnityEngine;

namespace TechC.VBattle.InGame.Gimmick
{
    /// <summary>
    /// バトル全体のギミックの管理
    /// </summary>
    public class BattleGimmickManager : Singleton<BattleGimmickManager>
    {
        [Header("通知タブ")]
        private readonly List<IGimmick> gimmicks = new();
        [SerializeField] private TabGimickController tabGimickController;
        [SerializeField] private WindowGimmickController windowGimmickController;

        protected override bool UseDontDestroyOnLoad => false;
        public override void Init()
        {
            base.Init();
            gimmicks.Add(tabGimickController);
            gimmicks.Add(windowGimmickController);
            foreach (var gimmick in gimmicks)
                gimmick.OnEnter();
        }
        protected override void OnRelease()
        {
            base.OnRelease();
            foreach (var gimmick in gimmicks)
                gimmick.OnExit();
        }


        private void Update()
        {
            foreach (var gimmick in gimmicks)
                gimmick.OnUpdate(Time.deltaTime);
        }
    }
}