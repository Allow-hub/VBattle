using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core;
using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Window;
using TechC.VBattle.InGame.Character;
using TechC.VBattle.InGame.Events;
using TechC.VBattle.InGame.Systems;
using TechC.VBattle.InGame.UI;
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
        #region 定数
        private const string KEYBOARD_CONTROL_SCHEME = "KeyboardScheme";
        private const string GAMEPAD_CONTROL_SCHEME = "PadScheme";
        #endregion

        #region バトル設定
        [SerializeField] private float battleTimeLimit = 60f; // 制限時間（秒）
        public float BattleTimeLimit => battleTimeLimit;
        private float remainingBattleTime;
        public float RemainingBattleTime => remainingBattleTime;
        private bool isTimeUpTriggered = false;
        #endregion

        #region デバッグ設定
        [Header("デバック関連")]
        [SerializeField] private bool isDebug = true;
        [SerializeField] private bool useNpc = false;

        [SerializeField] private Vector3 p1Rot;
        [SerializeField] private Vector3 p2Rot;
        [SerializeField] private Vector3 p1Pos;
        [SerializeField] private Vector3 p2Pos;
        [Header("IsDebugが有効の時に1Pから生成されるObj")]
        [SerializeField] private GameObject ameObj;
        [SerializeField] private GameObject teramiObj;

        [SerializeField] private CharacterData ameData;
        [SerializeField] private CharacterData teramiData;
        #endregion

        #region UI・カメラ
        [SerializeField] private Camera.CameraController cameraController;
        [SerializeField] private PlayerUIController player1UIController;
        [SerializeField] private PlayerUIController player2UIController;
        #endregion

        #region カウントダウン設定
        [SerializeField] private Vector2[] countdownPosition;
        [SerializeField] private Vector2[] countdownSize;
        [SerializeField] private int[] countdownFontSize; // フォントサイズ配列を追加
        private int countdownTimer = 3;
        private CancellationTokenSource countdownCts;
        #endregion

        #region ステート管理
        public InGameState InGameState => inGameState;
        private InGameState inGameState = InGameState.None;
        #endregion

        #region キャラクター・バトル管理
        private Character.CharacterController p1Controller;
        private Character.CharacterController p2Controller;
        public BattleEventBus BattleBus { get; private set; }
        private BattleJudge battleJudge;
        private HitStopController hitStopController;//イベントを使用しているので保持しておく必要がある
        #endregion

        #region ポーズ機能
        private bool isPaused = false;          // ポーズ状態フラグ
        public bool IsPaused => isPaused;       // 読み取り専用プロパティ
        public Func<bool> GetPauseStateFunc => () => isPaused;  // Funcデリゲート
        #endregion

        #region シングルトン設定
        protected override bool UseDontDestroyOnLoad => false;
        #endregion

        #region 初期化
        public override void Init()
        {
            base.Init();
            BattleBus = new BattleEventBus();
            hitStopController = new HitStopController(BattleBus);
            if (isDebug)
            {
                p1Controller = Instantiate(ameObj, p1Pos, Quaternion.Euler(p1Rot)).GetComponent<Character.CharacterController>();

                if (useNpc)
                    p2Controller = Instantiate(ameData.NpcPrefab, p2Pos, Quaternion.Euler(p2Rot)).GetComponent<Character.CharacterController>();
                else
                    p2Controller = Instantiate(ameObj, p2Pos, Quaternion.Euler(p2Rot)).GetComponent<Character.CharacterController>();

                p1Controller.Init(PlayerConstants.PLAYER_1_ID, Keyboard.current, false);
                p2Controller.Init(PlayerConstants.PLAYER_2_ID, Keyboard.current, useNpc); // useNpcフラグを使用
                
                battleJudge = new BattleJudge(p1Controller, p2Controller, BattleBus);

                if (GameDataBridge.I != null)
                    GameDataBridge.I.SetupPlayer(PlayerConstants.PLAYER_1_ID, new GameDataBridge.PlayerSetupData
                    {
                        PlayerIndex = PlayerConstants.PLAYER_1_ID,
                        DeviceName = Keyboard.current,
                        IsNPC = false,
                        SelectedCharacter = ameData
                    });

                if (GameDataBridge.I != null)
                    GameDataBridge.I.SetupPlayer(PlayerConstants.PLAYER_2_ID, new GameDataBridge.PlayerSetupData
                    {
                        PlayerIndex = PlayerConstants.PLAYER_2_ID,
                        DeviceName = Keyboard.current,
                        IsNPC = useNpc,
                        SelectedCharacter = ameData
                    });

                if (GameDataBridge.I != null)
                {
                    player1UIController.SetCharacterIcon(GameDataBridge.I.Player_1Setup.SelectedCharacter.CharacterName);
                    player2UIController.SetCharacterIcon(GameDataBridge.I.Player_2Setup.SelectedCharacter.CharacterName);
                }
                cameraController.SetupPlayers(p1Controller, p2Controller);

                ChangeState(InGameState.Battle);
            }
            else
            {
                // Player1は常にプレイヤー用プレハブ
                var p1Obj = Instantiate(GameDataBridge.I.Player_1Setup.SelectedCharacter.CharaPrefab, p1Pos, Quaternion.Euler(p1Rot));
                p1Controller = p1Obj.GetComponent<Character.CharacterController>();
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

                // Player2はNPCの場合、NPC専用プレハブを使用
                var p2Setup = GameDataBridge.I.Player_2Setup;
                GameObject p2Prefab = p2Setup.IsNPC ? p2Setup.SelectedCharacter.NpcPrefab : p2Setup.SelectedCharacter.CharaPrefab;
                var p2Obj = Instantiate(p2Prefab, p2Pos, Quaternion.Euler(p2Rot));
                p2Controller = p2Obj.GetComponent<Character.CharacterController>();

                // Player2のコントロールスキーム設定（プレイヤーの場合のみ）
                if (!GameDataBridge.I.Player_2Setup.IsNPC && GameDataBridge.I.Player_2Setup.DeviceName != null)
                {
                    var p2Input = p2Obj.GetComponent<PlayerInput>();
                    if (p2Input != null)
                    {
                        string p2Scheme = GameDataBridge.I.Player_2Setup.DeviceName is Gamepad ? GAMEPAD_CONTROL_SCHEME : KEYBOARD_CONTROL_SCHEME;
                        p2Input.SwitchCurrentControlScheme(p2Scheme, GameDataBridge.I.Player_2Setup.DeviceName);
                    }
                }

                p1Controller.Init(GameDataBridge.I.Player_1Setup.PlayerIndex, GameDataBridge.I.Player_1Setup.DeviceName, GameDataBridge.I.Player_1Setup.IsNPC);
                p2Controller.Init(GameDataBridge.I.Player_2Setup.PlayerIndex, GameDataBridge.I.Player_2Setup.DeviceName, GameDataBridge.I.Player_2Setup.IsNPC);
                player1UIController.SetCharacterIcon(GameDataBridge.I.Player_1Setup.SelectedCharacter.CharacterName);
                player2UIController.SetCharacterIcon(GameDataBridge.I.Player_2Setup.SelectedCharacter.CharacterName);
                battleJudge = new BattleJudge(p1Controller, p2Controller, BattleBus);
                cameraController.SetupPlayers(p1Controller, p2Controller);

                ChangeState(InGameState.Start);
            }
        }
        #endregion

        #region Unityライフサイクル
        private void Start()
        {
            BattleBus.Subscribe<PlayerOnDeathEvent>(e =>
            {
                if (!isTimeUpTriggered)
                {
                    isTimeUpTriggered = true;
                    ChangeState(InGameState.Result);
                }
            });
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
        #endregion

        #region ステート更新
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
        #endregion

        #region Startステート
        private void InitStartState()
        {
            if (InGameUIController.I != null)
                InGameUIController.I.SetResultCanvasActive(false);
            // 既存のカウントダウンをキャンセル
            countdownCts?.Cancel();
            countdownCts?.Dispose();
            countdownCts = new CancellationTokenSource();

            // カウントダウン中は入力を無効化
            if (p1Controller != null)
            {
                // PlayerのPlayerInputコンポーネントを無効化
                var p1PlayerInput = p1Controller.GetComponent<PlayerInput>();
                if (p1PlayerInput != null) p1PlayerInput.enabled = false;
            }
            if (p2Controller != null)
            {
                if (p2Controller.IsNPC)
                {
                    // NPCのBattleAIControllerを無効化
                    var aiController = p2Controller.GetComponent<Npc.BattleAIController>();
                    if (aiController != null) aiController.enabled = false;
                }
                else
                {
                    // PlayerのPlayerInputコンポーネントを無効化
                    var p2PlayerInput = p2Controller.GetComponent<PlayerInput>();
                    if (p2PlayerInput != null) p2PlayerInput.enabled = false;
                }
            }

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

        private void UpdateStartState() { }
        #endregion

        #region Battleステート
        private void InitBattleState()
        {
            remainingBattleTime = battleTimeLimit;
            isTimeUpTriggered = false;

            // NPC初期化
            if (p2Controller != null && p2Controller.IsNPC)
                SetupNpc(p2Controller, p1Controller.transform);

            // バトル開始時に入力を有効化
            if (p1Controller != null)
            {
                // PlayerのPlayerInputコンポーネントを有効化
                var p1PlayerInput = p1Controller.GetComponent<PlayerInput>();
                if (p1PlayerInput != null) p1PlayerInput.enabled = true;
            }
            if (p2Controller != null)
            {
                if (p2Controller.IsNPC)
                {
                    // NPCのBattleAIControllerを有効化
                    var aiController = p2Controller.GetComponent<Npc.BattleAIController>();
                    if (aiController != null) aiController.enabled = true;
                }
                else
                {
                    // PlayerのPlayerInputコンポーネントを有効化
                    var p2PlayerInput = p2Controller.GetComponent<PlayerInput>();
                    if (p2PlayerInput != null) p2PlayerInput.enabled = true;
                }
            }
        }

        private void UpdateBattleState()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                MenuManager.I.PressMenu(isPaused);
                isPaused = !isPaused;
            }
            if (isPaused) return;

            // ----- 制限時間減算 -----
            if (!isTimeUpTriggered)
            {
                remainingBattleTime -= Time.deltaTime;

                if (remainingBattleTime <= 0f)
                {
                    remainingBattleTime = 0f;
                    isTimeUpTriggered = true;

                    ChangeState(InGameState.Result);
                }
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
                SceneLoader.I.LoadSceneAsync(0).Forget();

        }
        #endregion

        #region Resultステート
        private void InitResultState()
        {
            if (p1Controller.CurrentHP > p2Controller.CurrentHP)
            {
                I.BattleBus.Publish(new PlayerOnDeathEvent() { PlayerIndex = PlayerConstants.PLAYER_2_ID });
            }
            else if (p2Controller.CurrentHP > p1Controller.CurrentHP)
            {
                I.BattleBus.Publish(new PlayerOnDeathEvent() { PlayerIndex = PlayerConstants.PLAYER_1_ID });
            }
            BattleBus.Publish(new PlayerOnDeathEvent() { PlayerIndex = 0 }); // 全員停止
            SceneLoader.I.SetCursorMode(true, CursorLockMode.None);
            if (InGameUIController.I != null)
                InGameUIController.I.SetResultCanvasActive(true);
        }

        private void UpdateResultState()
        {

        }
        #endregion

        #region 公開メソッド
        public void SetPauseState(bool pause) => isPaused = pause;
        #endregion
        
        #region NPC設定
        private void SetupNpc(Character.CharacterController character, Transform opponent)
        {
            if (character == null || !character.IsNPC) return;

            var aiController = character.GetComponent<Npc.BattleAIController>();

            var playerInput = character.GetComponent<PlayerInput>();
            if (playerInput != null)
                playerInput.enabled = false;

            aiController.Init(opponent);
        }
        #endregion
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