using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.InGame.Systems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC.VBattle.InGame
{
    /// <summary>
    /// インゲームの管理クラス
    /// </summary>
    public class InGameManager : Singleton<InGameManager>
    {
        [SerializeField] private bool isDebug = true;
        [SerializeField] private Vector3 p1Rot;
        [SerializeField] private Vector3 p2Rot;
        [SerializeField] private Vector3 p1Pos;
        [SerializeField] private Vector3 p2Pos;

        [SerializeField] private GameObject ameObj;
        private int countdownTimer = 3;
        private CancellationTokenSource countdownCts;
        public InGameState InGameState => inGameState;
        private InGameState inGameState = InGameState.None;
        public BattleEventBus BattleBus { get; private set; }
        private BattleJudge battleJudge;
        private HitStopManager hitStopManager;//イベントを使用しているので保持しておく必要がある
        private bool isPaused = false;          // ポーズ状態フラグ
        public bool IsPaused => isPaused;       // 読み取り専用プロパティ
        public Func<bool> GetPauseStateFunc => () => isPaused;  // Funcデリゲート
        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
            BattleBus = new BattleEventBus();
            hitStopManager = new HitStopManager(BattleBus);
            if (isDebug)
            {
                var p1 = Instantiate(ameObj, p1Pos, Quaternion.Euler(p1Rot)).GetComponent<Character.CharacterController>();
                var p2 = Instantiate(ameObj, p2Pos, Quaternion.Euler(p2Rot)).GetComponent<Character.CharacterController>();

                p1.Init(1, Keyboard.current, false);
                p2.Init(2, Keyboard.current, false);
                p2.GetComponent<PlayerInput>().enabled = false;

                battleJudge = new BattleJudge(p1, p2, BattleBus);
                ChangeState(InGameState.Battle);
            }
            else
            {
                if (GameDataBridge.I == null)
                {
                    Debug.LogError($"GameDataBridgeがnullです。Debugモードをオンにするか別シーンから開始してください");
                    return;
                }
                var p1 = Instantiate(GameDataBridge.I.Player_1Setup.SelectedCharacter.CharaPrefab, p1Pos, Quaternion.Euler(p1Rot)).GetComponent<Character.CharacterController>();
                var p2 = Instantiate(GameDataBridge.I.Player_2Setup.SelectedCharacter.CharaPrefab, p2Pos, Quaternion.Euler(p2Rot)).GetComponent<Character.CharacterController>();

                p1.Init(GameDataBridge.I.Player_1Setup.PlayerIndex, GameDataBridge.I.Player_1Setup.DeviceName, GameDataBridge.I.Player_1Setup.IsNPC);
                p2.Init(GameDataBridge.I.Player_2Setup.PlayerIndex, GameDataBridge.I.Player_2Setup.DeviceName, GameDataBridge.I.Player_2Setup.IsNPC);
                ChangeState(InGameState.Start);
            }
        }

        private void Update()
        {
            UpdateState();
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            countdownCts?.Cancel();
            countdownCts?.Dispose();
            battleJudge?.Dispose();
            BattleBus?.Clear();
        }


        private void UpdateState()
        {
            switch (inGameState)
            {
                case InGameState.Start:
                    UpdateStartState();
                    break;
                case InGameState.Battle:
                    UpdateBattleState();
                    break;
                case InGameState.Result:
                    UpdateResultState();
                    break;
            }
        }

        /// <summary>
        /// ステートを変更
        /// </summary>
        /// <param name="nextState">次のステート</param>
        private void ChangeState(InGameState nextState)
        {
            inGameState = nextState;
            switch (inGameState)
            {
                case InGameState.Start:
                    InitStartState();
                    break;
                case InGameState.Battle:
                    InitBattleState();
                    break;
                case InGameState.Result:
                    InitResultState();
                    break;
            }
        }

        private void InitStartState()
        {
            // 既存のカウントダウンをキャンセル
            countdownCts?.Cancel();
            countdownCts?.Dispose();
            countdownCts = new CancellationTokenSource();

            // カウントダウン開始
            CountdownAsync(countdownCts.Token).Forget();
        }

        /// <summary>
        /// ゲーム開始時のカウントダウン
        /// </summary>
        /// <param name="token">キャンセルのトークン</param>
        /// <returns></returns>
        private async UniTaskVoid CountdownAsync(CancellationToken token)
        {
            try
            {
                for (int i = countdownTimer; i > 0; i--)
                {
                    // ここに1秒ごとの処理を記述
                    Debug.Log($"カウントダウン: {i}");
                    // 例: UIの更新、サウンド再生など
                    // BattleBus.Publish(new CountdownEvent(i));

                    // 1秒待機
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
                }

                // カウントダウン終了後、Battleステートへ遷移
                Debug.Log("Battle Start!");
                ChangeState(InGameState.Battle);
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合の処理
                Debug.Log("Countdown cancelled");
            }
        }

        private void UpdateStartState()
        {

        }

        private void InitBattleState()
        {

        }

        private void UpdateBattleState()
        {

        }

        private void InitResultState()
        {

        }

        private void UpdateResultState()
        {

        }
    }

    /// <summary>
    /// インゲームのState
    /// </summary>
    public enum InGameState
    {
        None,
        Start,
        Battle,
        Result
    }
}