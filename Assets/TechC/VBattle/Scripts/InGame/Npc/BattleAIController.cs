using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TechC.VBattle.Core.Util;
using TechC.VBattle.Core.Extensions;

namespace TechC.VBattle.InGame.Npc
{
    /// <summary>
    /// 対戦AI制御クラス
    /// </summary>
    public class BattleAIController : MonoBehaviour
    {
        #region フィールド

        [Header("AI設定")]
        [Tooltip("難易度設定")]
        [SerializeField] private EnemyDifficulty difficulty = EnemyDifficulty.Normal;

        [Header("AI行動データ（難易度別）")]
        [Tooltip("Debug難易度用のデータ")]
        [SerializeField] private NpcDataSO debugData;
        
        [Tooltip("Easy難易度用のデータ")]
        [SerializeField] private NpcDataSO easyData;
        
        [Tooltip("Normal難易度用のデータ")]
        [SerializeField] private NpcDataSO normalData;

        private NpcDataSO npcData;
        private Transform opponent;
        private AIInputManager inputManager;
        private BattleAIStrategy strategy;
        private Character.CharacterController characterController;

        #endregion

        #region 内部状態

        private float lastActionTime;
        private BattleRange currentRange;
        private AIActionType currentAction;
        private bool isExecutingAction;
        private CancellationTokenSource aiCts;

        #endregion

        #region 初期化・破棄

        /// <summary>
        /// 外部から初期化
        /// </summary>
        /// <param name="opponentTransform">対戦相手のTransform</param>
        public void Init(Transform opponentTransform)
        {
            opponent = opponentTransform;

            if (inputManager == null)
                inputManager = GetComponent<AIInputManager>();

            if (characterController == null)
                characterController = GetComponent<Character.CharacterController>();

            // 難易度に応じてNpcDataSOを切り替え
            npcData = difficulty switch
            {
                EnemyDifficulty.Debug => debugData,
                EnemyDifficulty.Easy => easyData,
                EnemyDifficulty.Normal => normalData,
                _ => normalData
            };

            // NpcDataSOのチェック
            if (npcData == null)
            {
                CustomLogger.Error($"[{name}] 難易度 {difficulty} にNpcDataSOが設定されていません");
                return;
            }

            // 戦略を初期化（ScriptableObjectから）
            strategy = new BattleAIStrategy();
            strategy.Initialize(npcData);

            aiCts?.Cancel();
            aiCts?.Dispose();
            aiCts = new CancellationTokenSource();

            RunAILoopAsync(aiCts.Token).Forget();
        }

        private void OnDestroy()
        {
            aiCts?.Cancel();
            aiCts?.Dispose();
        }

        #endregion

        #region AIメインループ

        /// <summary>
        /// AI行動のメインループ
        /// </summary>
        private async UniTaskVoid RunAILoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // コンポーネントが無効の場合は処理をスキップ
                if (!enabled)
                {
                    await UniTask.Yield(token);
                    continue;
                }

                if (opponent == null || inputManager == null || strategy == null)
                {
                    await UniTask.Yield(token);
                    continue;
                }

                UpdateBattleRange();

                // アクション中（攻撃、ガード、ダメージ、空中）は次のアクションを開始しない
                if (!IsExecutingAction())
                {
                    await ExecuteAIActionAsync(token);
                    lastActionTime = Time.time;
                }

                await UniTask.Yield(token);
            }
        }

        /// <summary>
        /// 戦闘距離を更新
        /// </summary>
        private void UpdateBattleRange()
        {
            float distance = Vector3.Distance(transform.position, opponent.position);
            currentRange = strategy.GetBattleRange(distance);
        }

        /// <summary>
        /// キャラクターがアクション中かどうかを判定
        /// </summary>
        private bool IsExecutingAction()
        {
            if (characterController == null || characterController.StateMachine == null || characterController.StateMachine.CurrentState == null)
                return false;

            var currentState = characterController.StateMachine.CurrentState;
            
            // 攻撃中、ガード中、ダメージ中、空中は新しいアクションを開始しない
            return currentState is Character.AttackState or
                   Character.GuardState or
                   Character.DamageState or
                   Character.AirState;
        }

        /// <summary>
        /// AIの行動を実行
        /// </summary>
        private async UniTask ExecuteAIActionAsync(CancellationToken token)
        {
            currentAction = strategy.SelectAction(currentRange);
            await PerformActionAsync(currentAction, token);
        }

        #endregion

        #region 行動実行

        /// <summary>
        /// 選択された行動を実行
        /// </summary>
        private async UniTask PerformActionAsync(AIActionType actionType, CancellationToken token)
        {
            isExecutingAction = true;

            await DelayUtility.RunAfterDelayWithPause(
                npcData.ActionTimings.ReactionTime,
                () => { },
                InGameManager.I?.GetPauseStateFunc,
                token
            );

            switch (actionType)
            {
                case AIActionType.Approach:
                    await PerformApproachAsync(token);
                    break;

                case AIActionType.Retreat:
                    await PerformRetreatAsync(token);
                    break;

                case AIActionType.Attack:
                    await PerformAttackAsync(token);
                    break;

                case AIActionType.Guard:
                    await PerformGuardAsync(token);
                    break;

                case AIActionType.Jump:
                    await PerformJumpAsync(token);
                    break;

                case AIActionType.Crouch:
                    await PerformCrouchAsync(token);
                    break;

                case AIActionType.Wait:
                    await PerformWaitAsync(token);
                    break;
            }

            isExecutingAction = false;
        }

        /// <summary>
        /// 接近行動
        /// </summary>
        private async UniTask PerformApproachAsync(CancellationToken token)
        {
            Vector2 direction = GetDirectionToOpponent();
            inputManager.SetMoveInput(direction);

            await DelayUtility.RunAfterDelayWithPause(
                npcData.ActionTimings.ApproachTime,
                () => { },
                InGameManager.I?.GetPauseStateFunc,
                token
            );

            inputManager.SetMoveInput(Vector2.zero);
        }

        /// <summary>
        /// 後退行動
        /// </summary>
        private async UniTask PerformRetreatAsync(CancellationToken token)
        {
            Vector2 direction = -GetDirectionToOpponent();
            inputManager.SetMoveInput(direction);

            await DelayUtility.RunAfterDelayWithPause(
                npcData.ActionTimings.RetreatTime,
                () => { },
                InGameManager.I?.GetPauseStateFunc,
                token
            );

            inputManager.SetMoveInput(Vector2.zero);
        }

        /// <summary>
        /// 攻撃行動
        /// </summary>
        private async UniTask PerformAttackAsync(CancellationToken token)
        {
            Vector2 direction = GetAttackDirection();
            bool isWeak = Random.value < npcData.AttackSettings.WeakAttackChance;

            if (isWeak)
            {
                inputManager.SetWeakAttackInput(direction);
                await DelayUtility.RunAfterDelayWithPause(
                    npcData.ActionTimings.WeakAttackTime,
                    () => { },
                    InGameManager.I?.GetPauseStateFunc,
                    token
                );
                inputManager.ReleaseWeakAttack();
            }
            else
            {
                inputManager.SetStrongAttackInput(direction);
                await DelayUtility.RunAfterDelayWithPause(
                    npcData.ActionTimings.StrongAttackTime,
                    () => { },
                    InGameManager.I?.GetPauseStateFunc,
                    token
                );
                inputManager.ReleaseStrongAttack();
            }
        }

        /// <summary>
        /// ガード行動
        /// </summary>
        private async UniTask PerformGuardAsync(CancellationToken token)
        {
            inputManager.SetGuardInput(true);

            await DelayUtility.RunAfterDelayWithPause(
                npcData.ActionTimings.GuardTime,
                () => { },
                InGameManager.I?.GetPauseStateFunc,
                token
            );

            inputManager.SetGuardInput(false);
        }

        /// <summary>
        /// ジャンプ行動
        /// </summary>
        private async UniTask PerformJumpAsync(CancellationToken token)
        {
            inputManager.SetJumpInput(true);

            float attackDelay = npcData.ActionTimings.JumpTime * npcData.ActionTimings.AttackDelayRate;
            await DelayUtility.RunAfterDelayWithPause(
                attackDelay,
                () => { },
                InGameManager.I?.GetPauseStateFunc,
                token
            );

            if (Random.value < npcData.AttackSettings.JumpAttackChance)
            {
                bool isWeak = Random.value < npcData.AttackSettings.JumpWeakAttackChance;
                if (isWeak)
                {
                    inputManager.SetWeakAttackInput(Vector2.up);
                    await DelayUtility.RunAfterDelayWithPause(
                        npcData.ActionTimings.WeakAttackTime,
                        () => { },
                        InGameManager.I?.GetPauseStateFunc,
                        token
                    );
                    inputManager.ReleaseWeakAttack();
                }
                else
                {
                    inputManager.SetStrongAttackInput(Vector2.up);
                    await DelayUtility.RunAfterDelayWithPause(
                        npcData.ActionTimings.StrongAttackTime,
                        () => { },
                        InGameManager.I?.GetPauseStateFunc,
                        token
                    );
                    inputManager.ReleaseStrongAttack();
                }
            }

            await DelayUtility.RunAfterDelayWithPause(
                npcData.ActionTimings.JumpTime - attackDelay,
                () => { },
                InGameManager.I?.GetPauseStateFunc,
                token
            );

            inputManager.SetJumpInput(false);
        }

        /// <summary>
        /// しゃがみ行動
        /// </summary>
        private async UniTask PerformCrouchAsync(CancellationToken token)
        {
            inputManager.SetCrouchInput(true);

            float attackDelay = npcData.ActionTimings.CrouchTime * npcData.ActionTimings.AttackDelayRate;
            await DelayUtility.RunAfterDelayWithPause(
                attackDelay,
                () => { },
                InGameManager.I?.GetPauseStateFunc,
                token
            );

            if (Random.value < npcData.AttackSettings.CrouchAttackChance)
            {
                bool isWeak = Random.value < npcData.AttackSettings.CrouchWeakAttackChance;
                if (isWeak)
                {
                    inputManager.SetWeakAttackInput(Vector2.down);
                    await DelayUtility.RunAfterDelayWithPause(
                        npcData.ActionTimings.WeakAttackTime,
                        () => { },
                        InGameManager.I?.GetPauseStateFunc,
                        token
                    );
                    inputManager.ReleaseWeakAttack();
                }
                else
                {
                    inputManager.SetStrongAttackInput(Vector2.down);
                    await DelayUtility.RunAfterDelayWithPause(
                        npcData.ActionTimings.StrongAttackTime,
                        () => { },
                        InGameManager.I?.GetPauseStateFunc,
                        token
                    );
                    inputManager.ReleaseStrongAttack();
                }
            }

            await DelayUtility.RunAfterDelayWithPause(
                npcData.ActionTimings.CrouchTime - attackDelay,
                () => { },
                InGameManager.I?.GetPauseStateFunc,
                token
            );

            inputManager.SetCrouchInput(false);
        }

        /// <summary>
        /// 待機行動
        /// </summary>
        private async UniTask PerformWaitAsync(CancellationToken token)
        {
            await DelayUtility.RunAfterDelayWithPause(
                npcData.ActionTimings.WaitTime,
                () => { },
                InGameManager.I?.GetPauseStateFunc,
                token
            );
        }

        #endregion

        #region ユーティリティ

        /// <summary>
        /// 相手への方向ベクトルを取得
        /// </summary>
        private Vector2 GetDirectionToOpponent()
        {
            Vector3 direction = opponent.position - transform.position;
            return new Vector2(Mathf.Sign(direction.x), 0);
        }

        /// <summary>
        /// 攻撃する方向ベクトルをランダムに取得する
        /// </summary>
        private Vector2 GetAttackDirection()
        {
            float dx = opponent.position.x - transform.position.x;

            var directionProbability = npcData.DirectionProbability;
            float leftPercent = directionProbability.BaseLeftPercent;
            float rightPercent = directionProbability.BaseRightPercent;
            float upPercent = directionProbability.BaseUpPercent;
            float downPercent = directionProbability.BaseDownPercent;

            if (dx < 0)
            {
                leftPercent = directionProbability.PreferLeftPercent;
                rightPercent = directionProbability.LessRightPercent;
            }
            else if (dx > 0)
            {
                rightPercent = directionProbability.PreferRightPercent;
                leftPercent = directionProbability.LessLeftPercent;
            }

            float total = leftPercent + rightPercent + upPercent + downPercent;
            float rand = Random.Range(0f, total);

            if (rand < leftPercent) return Vector2.left;
            rand -= leftPercent;
            if (rand < rightPercent) return Vector2.right;
            rand -= rightPercent;
            if (rand < upPercent) return Vector2.up;
            return Vector2.down;
        }

        #endregion
    }
}
