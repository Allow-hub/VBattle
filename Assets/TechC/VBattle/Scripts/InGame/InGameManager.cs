using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Window;
using TechC.VBattle.InGame.Character;
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
        private const string KEYBOARD_CONTROL_SCHEME = "KeyboardScheme";
        private const string GAMEPAD_CONTROL_SCHEME = "PadScheme";
        
        [SerializeField] private bool isDebug = true;
        [SerializeField] private Vector3 p1Rot;
        [SerializeField] private Vector3 p2Rot;
        [SerializeField] private Vector3 p1Pos;
        [SerializeField] private Vector3 p2Pos;

        [SerializeField] private GameObject ameObj;
        [SerializeField] private CharacterData ameData;
        [SerializeField] private CharacterData teramiData;
        [SerializeField] private Camera.CameraController cameraController;
        
        [Header("カウンターテスト設定")]
        [Tooltip("有効にすると2Pが常にカウンター状態になります")]
        [SerializeField] private bool enableCounterTest = true;
        [Tooltip("カウンター発動時に実行する攻撃データ（isCounter=trueでnextChainが設定されている必要があります）")]
        [SerializeField] private AttackData counterAttackData;

        [SerializeField] private Vector2[] countdownPosition;
        [SerializeField] private Vector2[] countdownSize;
        [SerializeField] private int[] countdownFontSize; // フォントサイズ配列を追加
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
        
        // プレイヤー参照（カウンターテスト用）
        private Character.CharacterController p1Character;
        private Character.CharacterController p2Character;
 
        public override void Init()
        {
            base.Init();
            BattleBus = new BattleEventBus();
            hitStopController = new HitStopController(BattleBus);
            if (isDebug)
            {
                p1Character = Instantiate(ameObj, p1Pos, Quaternion.Euler(p1Rot)).GetComponent<Character.CharacterController>();
                p2Character = Instantiate(ameObj, p2Pos, Quaternion.Euler(p2Rot)).GetComponent<Character.CharacterController>();

                p1Character.Init(1, Keyboard.current, false);
                p2Character.Init(2, Keyboard.current, false);
                p2Character.GetComponent<PlayerInput>().enabled = false;

                battleJudge = new BattleJudge(p1Character, p2Character, BattleBus);

                if (GameDataBridge.I != null)
                    GameDataBridge.I.SetupPlayer(1, new GameDataBridge.PlayerSetupData
                    {
                        PlayerIndex = 1,
                        DeviceName = Keyboard.current,
                        IsNPC = false,
                        SelectedCharacter = ameData
                    });

                if (GameDataBridge.I != null)
                    GameDataBridge.I.SetupPlayer(2, new GameDataBridge.PlayerSetupData
                    {
                        PlayerIndex = 2,
                        DeviceName = Keyboard.current,
                        IsNPC = false,
                        SelectedCharacter = teramiData
                    });

                cameraController.SetupPlayers(p1Character, p2Character);
                ChangeState(InGameState.Battle);
            }
            else
            {
                var p1Obj = Instantiate(GameDataBridge.I.Player_1Setup.SelectedCharacter.CharaPrefab, p1Pos, Quaternion.Euler(p1Rot));
                p1Character = p1Obj.GetComponent<Character.CharacterController>();
                
                // Player1のコントロールスキーム設定
                if (GameDataBridge.I.Player_1Setup.DeviceName != null)
                {
                    var p1Input = p1Obj.GetComponent<PlayerInput>();
                    if (p1Input != null)
                    {
                        string p1Scheme = GameDataBridge.I.Player_1Setup.DeviceName is Gamepad ? GAMEPAD_CONTROL_SCHEME : KEYBOARD_CONTROL_SCHEME;
                        p1Input.SwitchCurrentControlScheme(p1Scheme, GameDataBridge.I.Player_1Setup.DeviceName);
                    }
                }
                
                var p2Obj = Instantiate(GameDataBridge.I.Player_2Setup.SelectedCharacter.CharaPrefab, p2Pos, Quaternion.Euler(p2Rot));
                p2Character = p2Obj.GetComponent<Character.CharacterController>();
                
                // Player2のコントロールスキーム設定
                if (GameDataBridge.I.Player_2Setup.DeviceName != null)
                {
                    var p2Input = p2Obj.GetComponent<PlayerInput>();
                    if (p2Input != null)
                    {
                        string p2Scheme = GameDataBridge.I.Player_2Setup.DeviceName is Gamepad ? GAMEPAD_CONTROL_SCHEME : KEYBOARD_CONTROL_SCHEME;
                        p2Input.SwitchCurrentControlScheme(p2Scheme, GameDataBridge.I.Player_2Setup.DeviceName);
                    }
                }

                p1Character.Init(GameDataBridge.I.Player_1Setup.PlayerIndex, GameDataBridge.I.Player_1Setup.DeviceName, GameDataBridge.I.Player_1Setup.IsNPC);
                p2Character.Init(GameDataBridge.I.Player_2Setup.PlayerIndex, GameDataBridge.I.Player_2Setup.DeviceName, GameDataBridge.I.Player_2Setup.IsNPC);
                
                battleJudge = new BattleJudge(p1Character, p2Character, BattleBus);
                cameraController.SetupPlayers(p1Character, p2Character);
                
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
            // カウンターテストモード
            if (enableCounterTest && p2Character != null && counterAttackData != null)
            {
                UpdateCounterTest();
            }
            
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

                // 3, 2, 1 のカウントダウン
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

                    // テキストを設定
                    basicWindow.SetText(i.ToString());

                    // フォントサイズを設定
                    int fontSize;
                    if (countdownFontSize != null && arrayIndex < countdownFontSize.Length)
                        fontSize = countdownFontSize[arrayIndex];
                    else if (countdownFontSize != null && countdownFontSize.Length > 0)
                        fontSize = countdownFontSize[countdownFontSize.Length - 1];// 配列の範囲外の場合は最後の要素を使用
                    else
                        fontSize = 300; // デフォルト値

                    basicWindow.SetFont(fontSize);

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

                    // 0.5秒待機
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);

                    // 下方向にアニメーション移動
                    try
                    {
                        await WindowUtility.MoveWindowInDirectionAsync(
                            basicWindow,
                            Vector2.down,
                            moveSpeedPerFrame: 50f,
                            intervalMs: 16,
                            texture: null,
                            durationSeconds: 0.5f
                        );
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"CountdownAsync: カウントダウンアニメーション移動で例外: {ex.Message}");
                    }

                    arrayIndex++; // 配列インデックスを進める
                }

                // 0のタイミングで「Start!」を表示
                token.ThrowIfCancellationRequested();

                var windowFactory = WindowFactory.I;
                if (windowFactory != null)
                {
                    var window = windowFactory.GetWindow(WindowFactory.WindowType.Basic);
                    if (window != null)
                    {
                        var basicWindow = window as BasicWindow;
                        if (basicWindow != null)
                        {
                            basicWindow.SetText("Start!");
                            basicWindow.SetFont(countdownFontSize.Last());

                            var hwndObj = window.Hwnd;
                            if (hwndObj != null && !hwndObj.Equals(IntPtr.Zero))
                            {
                                // 画面全体のサイズを取得
                                int screenWidth = Screen.currentResolution.width;
                                int screenHeight = Screen.currentResolution.height;

                                try
                                {
                                    WindowUtility.ResizeWindow((HWND)hwndObj, screenWidth, screenHeight);
                                    WindowUtility.MoveWindow((HWND)hwndObj, 0, 0);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogWarning($"CountdownAsync: Start表示時のMoveWindowで例外: {ex.Message}");
                                }
                            }

                            // Start!を1秒間表示
                            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

                            // 下方向にアニメーション移動してからHide
                            try
                            {
                                await WindowUtility.MoveWindowInDirectionAsync(
                                    basicWindow,
                                    Vector2.down,
                                    moveSpeedPerFrame: 50f,
                                    intervalMs: 16,
                                    texture: null,
                                    durationSeconds: 1f
                                );
                                basicWindow.Hide();
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"CountdownAsync: アニメーション移動で例外: {ex.Message}");
                            }
                        }
                    }
                }

                // カウントダウン終了後、Battleステートへ遷移
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
                SceneLoader.I.LoadSceneAsync(0).Forget();

            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                isPaused = !isPaused;
                // CustomLogger.Info($"ポーズ切り替え: {isPaused}");
            }
        }

        private void InitResultState()
        {

        }

        private void UpdateResultState()
        {

        }

        public void SetPauseState(bool pause) => isPaused = pause;
        
        /// <summary>
        /// カウンターテストモードの更新処理
        /// 2Pを常にカウンター状態に保つ
        /// </summary>
        private void UpdateCounterTest()
        {
            // 2Pがカウンター状態でない場合、カウンター攻撃を設定
            if (!p2Character.CanCounter)
            {
                p2Character.SetCanCounter(true);
                p2Character.SetCounterAction(() =>
                {
                    Debug.Log("P2 カウンター発動！");
                    
                    // counterAttackDataが設定されている場合、それを使って攻撃
                    if (counterAttackData != null)
                    {
                        var attackData = counterAttackData.nextChain != null ? counterAttackData.nextChain : counterAttackData;
                        ExecuteCounterAttackAsync(attackData).Forget();
                    }
                    else
                    {
                        // counterAttackDataが未設定の場合、デフォルトの攻撃
                        ExecuteCounterAttackAsync(null).Forget();
                    }
                });
            }
        }
        
        /// <summary>
        /// カウンター攻撃を非同期で実行
        /// </summary>
        private async UniTaskVoid ExecuteCounterAttackAsync(AttackData attackData)
        {
            // 数フレーム待機して、現在の処理が完全に終了するのを待つ
            await UniTask.DelayFrame(3, PlayerLoopTiming.Update);
            
            // 2Pが攻撃可能な状態か確認
            if (p2Character != null && p2Character.StateMachine != null)
            {
                // 攻撃を実行
                p2Character.Attack(AttackType.Weak, AttackDirection.Neutral);
                
                if (attackData != null)
                {
                    Debug.Log($"P2 カウンター攻撃実行: {attackData.attackName}");
                }
                else
                {
                    Debug.Log("P2 カウンター攻撃実行: デフォルト攻撃");
                }
            }
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