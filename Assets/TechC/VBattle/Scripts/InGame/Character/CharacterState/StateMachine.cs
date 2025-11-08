using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// CharacterState用のStateMachine
    /// </summary>
    public class StateMachine
    {
        private CharacterState _currentState;
        private CancellationTokenSource _cts;
        private CancellationTokenSource _ctsForState;

        public CharacterState CurrentState => _currentState;

        /// <summary>
        /// 外部から状態を変更
        /// </summary>
        public void ChangeState(CharacterState nextState)
        {
            if (nextState == null)
                return;

            // 現在の状態をキャンセル
            if (_ctsForState != null)
            {
                _ctsForState.Cancel();
                _ctsForState.Dispose();
                _ctsForState = null;
            }

            var prev = _currentState;
            _currentState?.OnExit();
            _currentState = nextState;
            _currentState?.OnEnter(prev);

            // 新しい状態用のCancellationTokenを作成
            _ctsForState = new CancellationTokenSource();
        }

        public async UniTaskVoid Run(CharacterState start)
        {
            if (start == null)
                return;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            // 初期状態を設定（ChangeStateを使わずに直接設定）
            _currentState = start;
            _currentState.OnEnter(null);
            _ctsForState = new CancellationTokenSource();

            CharacterState nextState = start;

            while (!token.IsCancellationRequested)
            {
                // 外部からChangeStateが呼ばれて状態が変わった場合
                if (_currentState != nextState)
                {
                    nextState = _currentState;
                }

                if (_ctsForState == null)
                    _ctsForState = new CancellationTokenSource();

                using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, _ctsForState.Token);
                var ct = linked.Token;

                try
                {
                    if (_currentState == null)
                        break;

                    // 状態のUpdateを実行し、次の状態を取得
                    nextState = await _currentState.OnUpdate(ct);

                    // OnUpdateから返された状態がnullでないか確認
                    if (nextState == null)
                    {
                        nextState = _currentState; // 現在の状態を維持
                    }

                    // 状態が変わる場合は遷移
                    if (nextState != _currentState)
                    {
                        ChangeState(nextState);
                    }
                }
                catch (System.OperationCanceledException)
                {
                    // キャンセルされた場合は現在の状態を維持
                    nextState = _currentState;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"State update error in {_currentState?.GetType().Name}: {ex}");
                    nextState = _currentState;
                }
            }
        }

        public void Start(CharacterState start)
        {
            Run(start).Forget();
        }

        public void Cancel()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _ctsForState?.Cancel();
            _ctsForState?.Dispose();
        }
    }
}