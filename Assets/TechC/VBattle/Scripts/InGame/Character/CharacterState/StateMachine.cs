using System.Threading;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Util;

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
        /// 状態を実行するメインループ
        /// </summary>
        /// <param name="start">始まりのステート</param>
        /// <returns></returns>
        public async UniTaskVoid Run(CharacterState start)
        {
            if (start == null)
                return;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _currentState = start;
            _currentState.OnEnter(null);
            _ctsForState = new CancellationTokenSource();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_currentState == null)
                        break;

                    // 状態専用のトークンを使用
                    var stateToken = _ctsForState.Token;

                    // OnUpdateを実行
                    var nextState = await _currentState.OnUpdate(stateToken);

                    if (nextState == null)
                    {
                        nextState = _currentState;
                    }

                    // 状態遷移が必要な場合
                    if (nextState != _currentState)
                    {
                        // OnUpdateが正常に完了してから遷移
                        ChangeState(nextState);
                    }
                }
                catch (System.OperationCanceledException)
                {
                    // 外部からChangeStateが呼ばれた場合
                    CustomLogger.Info($"{_currentState?.GetType().Name} was cancelled", LogTagUtil.TagState);
                    // 既に新しい状態に遷移済みなので、次のループで新しい状態のOnUpdateが呼ばれる
                }
                catch (System.Exception ex)
                {
                    CustomLogger.Error($"State update error in {_currentState?.GetType().Name}: {ex}", LogTagUtil.TagState);
                }

                await UniTask.Yield(token);
            }
        }

        /// <summary>
        /// 外部から状態を変更
        /// </summary>
        public void ChangeState(CharacterState nextState)
        {
            if (nextState == null)
                return;
            CustomLogger.Info($"ChangeState: {_currentState?.GetType().Name} -> {nextState.GetType().Name}", LogTagUtil.TagState);

            // 現在の状態をキャンセル
            _ctsForState?.Cancel();
            _ctsForState?.Dispose();

            var prev = _currentState;
            _currentState?.OnExit();
            _currentState = nextState;
            _currentState?.OnEnter(prev);

            // 新しい状態用のCancellationTokenを作成
            _ctsForState = new CancellationTokenSource();
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