using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Util;
using TechC.VBattle.Core;

namespace TechC.VBattle.InGame.Npc
{
    /// <summary>
    /// 対戦AI制御クラス
    /// </summary>
    public class BattleAIController : MonoBehaviour
    {
        [Header("AI設定")]
        private Transform opponent;
        private AIInputManager inputManager;
        private BattleAIStrategy strategy;

        [Header("行動設定")]
        [SerializeField] private float actionInterval = 0.5f;
        [SerializeField] private float reactionTime = 0.1f;

        [Header("難易度")]
        [SerializeField] private EnemyDifficulty difficulty = EnemyDifficulty.Normal;

        [Header("【重要】各行動の時間（※難易度をDEBUGにすることで反映）")]
        [Tooltip("接近行動の継続時間（秒）")]
        [SerializeField] private float approachTime = 0.3f;
        [Tooltip("後退行動の継続時間（秒）")]
        [SerializeField] private float retreatTime = 0.3f;
        [Tooltip("弱攻撃の入力継続時間（秒）")]
        [SerializeField] private float weakAttackTime = 0.15f;
        [Tooltip("強攻撃の入力継続時間（秒）")]
        [SerializeField] private float strongAttackTime = 0.3f;
        [Tooltip("ガードの継続時間（秒）")]
        [SerializeField] private float guardTime = 0.3f;
        [Tooltip("ジャンプの入力継続時間（秒）")]
        [SerializeField] private float jumpTime = 0.12f;
        [Tooltip("しゃがみの継続時間（秒）")]
        [SerializeField] private float crouchTime = 0.25f;
        [Tooltip("待機行動の継続時間（秒）")]
        [SerializeField] private float waitTime = 0.25f;

        [Header("通常攻撃設定")]
        [SerializeField, Range(0, 1)] private float weakAttackChance = 0.7f;

        [Header("攻撃方向の確率（通常時）")]
        [Tooltip("左方向への攻撃確率（通常時）")]
        [SerializeField, ReadOnly] private float baseLeftPercent = 25f;

        [Tooltip("右方向への攻撃確率（通常時）")]
        [SerializeField, ReadOnly] private float baseRightPercent = 25f;

        [Tooltip("上方向への攻撃確率（通常時）")]
        [SerializeField, ReadOnly] private float baseUpPercent = 25f;

        [Tooltip("下方向への攻撃確率（通常時）")]
        [SerializeField, ReadOnly] private float baseDownPercent = 25f;

        [Header("攻撃方向の確率（優遇時）")]
        [SerializeField] private float preferLeftPercent = 40f;
        [SerializeField] private float preferRightPercent = 40f;
        [SerializeField] private float lessLeftPercent = 10f;
        [SerializeField] private float lessRightPercent = 10f;

        [Header("ジャンプ攻撃設定")]
        [SerializeField, Range(0, 1)] private float jumpAttackChance = 0.8f;
        [SerializeField, Range(0, 1)] private float jumpWeakAttackChance = 0.7f;

        [Header("しゃがみの攻撃設定")]
        [SerializeField, Range(0, 1)] private float crouchAttackChance = 0.6f;
        [SerializeField, Range(0, 1)] private float crouchWeakAttackChance = 0.7f;

        private const float ATTACK_DELAY_RATE = 0.5f;

        private float lastActionTime;
        private BattleRange currentRange;
        private AIActionType currentAction;
        private bool isExecutingAction;
        private CancellationTokenSource aiCts;

        /// <summary>
        /// 外部から初期化
        /// </summary>
        /// <param name="opponentTransform">対戦相手のTransform</param>
        public void Init(Transform opponentTransform)
        {
            opponent = opponentTransform;

            if (inputManager == null)
                inputManager = GetComponent<AIInputManager>();

            if (strategy == null)
                strategy = GetComponent<BattleAIStrategy>();

            ApplyDifficultySettings();

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

        /// <summary>
        /// AI行動のメインループ
        /// </summary>
        private async UniTaskVoid RunAILoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                while (InGameManager.I != null && InGameManager.I.IsPaused)
                {
                    await UniTask.Yield(token);
                }

                if (opponent == null || inputManager == null || strategy == null)
                {
                    await UniTask.Yield(token);
                    continue;
                }

                UpdateBattleRange();

                if (Time.time - lastActionTime >= actionInterval && !isExecutingAction)
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
        /// AIの行動を実行
        /// </summary>
        private async UniTask ExecuteAIActionAsync(CancellationToken token)
        {
            currentAction = strategy.SelectAction(currentRange);
            CustomLogger.Info($"AI Action: {currentAction}", LogTagUtil.TagNpc);
            await PerformActionAsync(currentAction, token);
        }

        /// <summary>
        /// 選択された行動を実行
        /// </summary>
        private async UniTask PerformActionAsync(AIActionType actionType, CancellationToken token)
        {
            isExecutingAction = true;

            await DelayUtility.RunAfterDelayWithPause(
                reactionTime,
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
                approachTime,
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
                retreatTime,
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
            bool isWeak = Random.value < weakAttackChance;

            if (isWeak)
            {
                inputManager.SetWeakAttackInput(direction);
                await DelayUtility.RunAfterDelayWithPause(
                    weakAttackTime,
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
                    strongAttackTime,
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
                guardTime,
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

            float attackDelay = jumpTime * ATTACK_DELAY_RATE;
            await DelayUtility.RunAfterDelayWithPause(
                attackDelay,
                () => { },
                InGameManager.I?.GetPauseStateFunc,
                token
            );

            if (Random.value < jumpAttackChance)
            {
                bool isWeak = Random.value < jumpWeakAttackChance;
                if (isWeak)
                {
                    inputManager.SetWeakAttackInput(Vector2.up);
                    await DelayUtility.RunAfterDelayWithPause(
                        weakAttackTime,
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
                        strongAttackTime,
                        () => { },
                        InGameManager.I?.GetPauseStateFunc,
                        token
                    );
                    inputManager.ReleaseStrongAttack();
                }
            }

            await DelayUtility.RunAfterDelayWithPause(
                jumpTime - attackDelay,
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

            float attackDelay = crouchTime * ATTACK_DELAY_RATE;
            await DelayUtility.RunAfterDelayWithPause(
                attackDelay,
                () => { },
                InGameManager.I?.GetPauseStateFunc,
                token
            );

            if (Random.value < crouchAttackChance)
            {
                bool isWeak = Random.value < crouchWeakAttackChance;
                if (isWeak)
                {
                    inputManager.SetWeakAttackInput(Vector2.down);
                    await DelayUtility.RunAfterDelayWithPause(
                        weakAttackTime,
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
                        strongAttackTime,
                        () => { },
                        InGameManager.I?.GetPauseStateFunc,
                        token
                    );
                    inputManager.ReleaseStrongAttack();
                }
            }

            await DelayUtility.RunAfterDelayWithPause(
                crouchTime - attackDelay,
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
                waitTime,
                () => { },
                InGameManager.I?.GetPauseStateFunc,
                token
            );
        }

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

            float leftPercent = baseLeftPercent;
            float rightPercent = baseRightPercent;
            float upPercent = baseUpPercent;
            float downPercent = baseDownPercent;

            if (dx < 0)
            {
                leftPercent = preferLeftPercent;
                rightPercent = lessRightPercent;
            }
            else if (dx > 0)
            {
                rightPercent = preferRightPercent;
                leftPercent = lessLeftPercent;
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

        /// <summary>
        /// 難易度に応じて、CPUのパラメータを変更する
        /// </summary>
        private void ApplyDifficultySettings()
        {
            switch (difficulty)
            {
                case EnemyDifficulty.Debug:
                    break;
                case EnemyDifficulty.Easy:
                    actionInterval = 0.8f;
                    reactionTime = 0.3f;
                    approachTime = 0.4f;
                    retreatTime = 0.4f;
                    weakAttackTime = 0.18f;
                    strongAttackTime = 0.35f;
                    guardTime = 0.35f;
                    jumpTime = 0.22f;
                    crouchTime = 0.36f;
                    waitTime = 0.35f;
                    strategy.SetPersonality(0.7f, 1.2f, 0.8f);
                    break;
                case EnemyDifficulty.Normal:
                    actionInterval = 0.5f;
                    reactionTime = 0.1f;
                    approachTime = 0.3f;
                    retreatTime = 0.3f;
                    weakAttackTime = 0.15f;
                    strongAttackTime = 0.3f;
                    guardTime = 0.3f;
                    jumpTime = 0.16f;
                    crouchTime = 0.28f;
                    waitTime = 0.25f;
                    strategy.SetPersonality(1.0f, 1.0f, 1.0f);
                    break;
            }
        }
    }
}
