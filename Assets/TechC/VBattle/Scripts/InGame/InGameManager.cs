using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Window;
using TechC.VBattle.InGame.Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using Windows.Win32.Foundation;

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

        [SerializeField] private Vector2[] countdownPosition;
        [SerializeField] private Vector2[] countdownSize;
        private int countdownTimer = 3;
        private CancellationTokenSource countdownCts;
        public InGameState InGameState => inGameState;
        private InGameState inGameState = InGameState.None;
        public BattleEventBus BattleBus { get; private set; }
        private BattleJudge battleJudge;
        private HitStopController hitStopController;//イベントを使用しているので保持しておく必要がある
        private bool isPaused = false;          // ポーズ状態フラグ
        public bool IsPaused => isPaused;       // 読み取り専用プロパティ
        public Func<bool> GetPauseStateFunc => () => isPaused;  // Funcデリゲート
        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
            BattleBus = new BattleEventBus();
            hitStopController = new HitStopController(BattleBus);
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
                // var p1 = Instantiate(GameDataBridge.I.Player_1Setup.SelectedCharacter.CharaPrefab, p1Pos, Quaternion.Euler(p1Rot)).GetComponent<Character.CharacterController>();
                // var p2 = Instantiate(GameDataBridge.I.Player_2Setup.SelectedCharacter.CharaPrefab, p2Pos, Quaternion.Euler(p2Rot)).GetComponent<Character.CharacterController>();

                // p1.Init(GameDataBridge.I.Player_1Setup.PlayerIndex, GameDataBridge.I.Player_1Setup.DeviceName, GameDataBridge.I.Player_1Setup.IsNPC);
                // p2.Init(GameDataBridge.I.Player_2Setup.PlayerIndex, GameDataBridge.I.Player_2Setup.DeviceName, GameDataBridge.I.Player_2Setup.IsNPC);
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
                if (countdownPosition == null || countdownPosition.Length == 0) return;
                
                // カウントダウン用のインデックス（0から始まる）
                int arrayIndex = 0;
                
                for (int i = countdownTimer; i > 0; i--)
                {
                    // キャンセルチェック
                    token.ThrowIfCancellationRequested();

                    // WindowFactory/I の存在確認
                    var wf = WindowFactory.I;
                    if (wf == null)
                    {
                        Debug.LogWarning("CountdownAsync: WindowFactory.I が null です。ウィンドウ移動をスキップします。");
                        await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
                        arrayIndex++;
                        continue;
                    }

                    var w = wf.GetWindow(WindowFactory.WindowType.Basic);
                    if (w == null)
                    {
                        Debug.LogWarning("CountdownAsync: WindowFactory.GetWindow が null を返しました。");
                        await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
                        arrayIndex++;
                        continue;
                    }

                    var basicWindow = w as BasicWindow;
                    if (basicWindow == null)
                    {
                        Debug.LogWarning("CountdownAsync: 取得したウィンドウが BasicWindow にキャストできません。");
                        await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
                        arrayIndex++;
                        continue;
                    }
                    
                    // テキスト、位置、サイズを設定
                    basicWindow.SetText(i.ToString());

                    // Hwnd の確認（IntPtr.Zero なら無効）
                    var hwndObj = w.Hwnd;
                    if (hwndObj == null || hwndObj.Equals(IntPtr.Zero))
                    {
                        Debug.LogWarning("CountdownAsync: window.Hwnd が null または IntPtr.Zero です。");
                        await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
                        arrayIndex++;
                        continue;
                    }

                    // arrayIndex を使用して配列から位置とサイズを取得
                    Vector2 pos;
                    Vector2 size;
                    if (countdownPosition != null && arrayIndex < countdownPosition.Length)
                    {
                        pos = countdownPosition[arrayIndex];
                        size = countdownSize[arrayIndex];
                    }
                    else if (countdownPosition != null && countdownPosition.Length > 0)
                    {
                        // 配列の範囲外の場合は最後の要素を使用
                        pos = countdownPosition[countdownPosition.Length - 1];
                        size = countdownSize[countdownSize.Length - 1];
                    }
                    else
                    {
                        pos = Vector2.zero;
                        size = new Vector2(300, 300);
                    }

                    try
                    {
                        WindowUtility.ResizeWindow((HWND)hwndObj, (int)size.x, (int)size.y);
                        WindowUtility.MoveWindow((HWND)hwndObj, (int)pos.x, (int)pos.y);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"CountdownAsync: MoveWindow で例外: {ex.Message}");
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);// 1秒待機
                    arrayIndex++; // 配列インデックスを進める
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
            catch (Exception ex)
            {
                Debug.LogException(ex);
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
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                SceneLoader.I.LoadSceneAsync(0).Forget();
            }
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