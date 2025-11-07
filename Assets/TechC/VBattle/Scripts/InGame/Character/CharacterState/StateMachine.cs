using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    public class StateMachine
    {
        public class State
        {
            public delegate void OnEnterCallback(State prev);
            public delegate UniTask<State> OnUpdateCallback(CancellationToken ct);
            public delegate void OnExitCallback();

            public OnEnterCallback OnEnter;
            public OnUpdateCallback OnUpdate;
            public OnExitCallback OnExit;
        }

        private State _currentState;
        private CancellationTokenSource _cts;
        private CancellationTokenSource _ctsForState;

        private void ChangeState(State nextState)
        {
            if (_ctsForState != null)
            {
                _ctsForState.Cancel();
                _ctsForState.Dispose();
            }

            var prev = _currentState;
            _currentState?.OnExit?.Invoke();
            _currentState = nextState;
            _currentState?.OnEnter?.Invoke(prev);
        }

        public async UniTaskVoid Run(State start)
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            var state = start;

            while (!token.IsCancellationRequested)
            {
                ChangeState(state);

                _ctsForState = new CancellationTokenSource();
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, _ctsForState.Token);
                var ct = linked.Token;

                if (_currentState.OnUpdate == null)
                {
                    // Updateが未定義なら永遠にEndOfFrameを待つ
                    while (!ct.IsCancellationRequested)
                    {
                        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, ct);
                    }
                }
                else
                {
                    try
                    {
                        // awaitで次のStateを受け取る
                        state = await _currentState.OnUpdate.Invoke(ct);
                    }
                    catch (System.OperationCanceledException)
                    {
                        // キャンセルされた場合はループを抜ける
                        break;
                    }
                }
            }
        }

        public void Start(State start)
        {
            Run(start).Forget(); // 非同期開始
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