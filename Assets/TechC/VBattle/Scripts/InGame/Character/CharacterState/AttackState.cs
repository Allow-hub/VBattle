using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Util;
using TechC.VBattle.InGame.Events;
using TechC.VBattle.Systems;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 攻撃ステート、ここでの連鎖は同一攻撃タイプ内での連続攻撃を指す
    /// </summary>
    public class AttackState : CharacterState
    {
        // カウンター受付時間（すべてのカウンター攻撃で共通）
        private const float COUNTER_START_TIME = 0.0f;  // 攻撃開始からカウンター受付開始までの時間
        private const float COUNTER_END_TIME = 1.0f;    // 攻撃開始からカウンター受付終了までの時間（テスト用に5秒に延長）
        
        private AttackData currentAttackData;
        private bool canCancel = false;
        private bool isAirAttack = false;
        private bool isChainRequested = false;
        private int chain = 0;
        private bool isCounterAttack = false; // カウンター攻撃として実行された攻撃かどうか
        public AttackState(CharacterController controller) : base(controller) { }

        public override bool CanExecuteCommand<T>(T command)
        {
            // 攻撃コマンドでなければリターン
            if (command.Type != CommandType.Attack) return false;
            // キャンセル不可ならリターン
            if (!canCancel) return false;
            // 連鎖攻撃不可ならリターン
            if (!currentAttackData.canChain || currentAttackData.nextChain == null) return false;
            // ここまで来たら連鎖攻撃可能
            isChainRequested = true;
            return true;
        }

        public override void OnEnter(CharacterState prevState)
        {
            isChainRequested = false;
            
            // カウンター攻撃かどうかをチェック
            isCounterAttack = controller.IsExecutingCounterAttack;
            
            //空中攻撃は派生させない予定なので無理やり矯正する形で
            if (!controller.IsGrounded())
            {
                controller.Anim.SetInteger(AnimatorParam.AttackType, 2);//Air
                controller.Anim.SetInteger(AnimatorParam.AttackDirection, 0);//Neutral
                isAirAttack = true;
                currentAttackData = controller.AttackSet.GetAttackData(AttackType.Air, AttackDirection.Neutral);
            }
            else
            {
                //地上での攻撃
                controller.Anim.SetInteger(AnimatorParam.AttackType, (int)controller.CurrentAttackType);
                controller.Anim.SetInteger(AnimatorParam.AttackDirection, (int)controller.CurrentAttackDirection);
                isAirAttack = false;
                currentAttackData = controller.AttackSet.GetAttackData(controller.CurrentAttackType, controller.CurrentAttackDirection);
            }
            controller.Anim.speed = currentAttackData.animationSpeed;
            AnimatorUtil.SetAnimatorBoolExclusive(controller.Anim, AnimatorParam.IsAttacking);
        }

        public override async UniTask<CharacterState> OnUpdate(CancellationToken ct)
        {
            try
            {
                // 攻撃ループ
                while (true)
                {
                    isChainRequested = false;

                    float attackTime = currentAttackData.attackDuration;
                    float recoveryTime = currentAttackData.recoveryDuration;

                    // 攻撃開始（キャンセル不可）
                    canCancel = false;
                    
                    // isCounterフラグに基づいてカウンター機能を有効にする（カウンター攻撃ではない場合のみ）
                    bool shouldEnableCounter = currentAttackData.isCounter && !isCounterAttack;
                    
                    if (shouldEnableCounter)
                    {
                        // カウンター受付開始（COUNTER_START_TIME = 0なので即座に開始）
                        controller.SetCanCounter(true);
                        SetupCounterAction();
                    }
                    else if (isCounterAttack)
                    {
                        // カウンター攻撃なのでカウンター機能無効
                    }

                    // hitTimingまでの残り時間を待機
                    float remainingToHitTiming = currentAttackData.hitTiming - (currentAttackData.isCounter ? COUNTER_START_TIME : 0);
                    if (remainingToHitTiming > 0)
                        await UniTask.Delay(TimeSpan.FromSeconds(remainingToHitTiming), cancellationToken: ct);

                    // 攻撃Prefab生成と判定を実行
                    CreateAttackObject();
                    PerformHitDetection();
                    
                    // カウンター受付終了タイミングまでの待機
                    if (currentAttackData.isCounter && COUNTER_END_TIME > currentAttackData.hitTiming)
                    {
                        float remainingToCounterEnd = COUNTER_END_TIME - currentAttackData.hitTiming;
                        if (remainingToCounterEnd > 0)
                            await UniTask.Delay(TimeSpan.FromSeconds(remainingToCounterEnd), cancellationToken: ct);
                        
                        // カウンター受付終了
                        controller.SetCanCounter(false);
                        controller.ResetCounterAction();
                    }

                    // cancelStartTimeまでの残り時間を待機
                    float counterEndOrHitTiming = currentAttackData.isCounter ? Mathf.Max(COUNTER_END_TIME, currentAttackData.hitTiming) : currentAttackData.hitTiming;
                    float remainingToCancelStart = currentAttackData.cancelStartTime - counterEndOrHitTiming;
                    if (remainingToCancelStart > 0)
                        await UniTask.Delay(TimeSpan.FromSeconds(remainingToCancelStart), cancellationToken: ct);

                    // キャンセル可能タイミング
                    canCancel = true;
                    float cancelWindow = currentAttackData.cancelEndTime - currentAttackData.cancelStartTime;
                    if (cancelWindow > 0)
                        await UniTask.Delay(TimeSpan.FromSeconds(cancelWindow), cancellationToken: ct);

                    // キャンセル可能時間が終了
                    canCancel = false;
                    // 連鎖攻撃がリクエストされているかチェック
                    if (isChainRequested && currentAttackData.canChain && currentAttackData.nextChain != null)
                    {
                        // 次の連鎖攻撃に移行
                        currentAttackData = currentAttackData.nextChain;

                        chain++;
                        // アニメーションを更新
                        controller.Anim.SetInteger(AnimatorParam.Chain, chain);
                        controller.Anim.speed = currentAttackData.animationSpeed;
                        // 次のループで新しい攻撃を実行
                        continue;
                    }

                    // 連鎖がない場合は残りの硬直を待つ
                    float remainingAttack = attackTime - currentAttackData.cancelEndTime;
                    if (remainingAttack > 0)
                        await UniTask.Delay(TimeSpan.FromSeconds(remainingAttack), cancellationToken: ct);

                    // 硬直フレーム（recoveryDuration）
                    await UniTask.Delay(TimeSpan.FromSeconds(recoveryTime), cancellationToken: ct);

                    // 攻撃終了
                    break;
                }
            }
            catch (OperationCanceledException) // 攻撃が何かしらの要因によって中断されたとき
            {
                CustomLogger.Error($"[AttackState] キャンセルされました (プレイヤー{controller.PlayerIndex})", LogTagUtil.TagState);
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"AttackState error: {ex.Message}\n{ex.StackTrace}");
                return isAirAttack ? controller.GetState<AirState>() : controller.GetState<NeutralState>();
            }

            // 攻撃終了後の状態遷移
            return isAirAttack ? controller.GetState<AirState>() : controller.GetState<NeutralState>();
        }
        public override void OnExit()
        {
            controller.Anim.SetInteger(AnimatorParam.Chain, 0);//連鎖リセット
            controller.Anim.speed = controller.IdleAnimSpeed;
            controller.Anim.SetBool(AnimatorParam.IsAttacking, false);
            canCancel = false;
            isChainRequested = false;
            chain = 0;
            
            // カウンター状態をリセット（念のため）
            controller.SetCanCounter(false);
            controller.ResetCounterAction();
        }

        /// <summary>
        /// カウンター発動時のアクションを設定
        /// </summary>
        private void SetupCounterAction()
        {
            if (currentAttackData == null) return;
            
            // カウンター攻撃実行中はカウンターアクションを設定しない（無限ループ防止）
            if (controller.IsExecutingCounterAttack)
            {
                return;
            }
            
            // isCounterフラグに基づいてカウンターアクションを設定
            if (!currentAttackData.isCounter) return;

            controller.SetCounterAction(() =>
            {
                // カウンター受付終了
                controller.SetCanCounter(false);
                controller.ResetCounterAction();
                
                // InGameManagerのテスト機能と同じように、カウンター攻撃を実行
                var counterAttackData = GetCounterAttackData();
                if (counterAttackData != null)
                {
                    ExecuteCounterAttackAsync(counterAttackData).Forget();
                }
                else
                {
                    // デフォルトの攻撃を実行
                    ExecuteCounterAttackAsync(null).Forget();
                }
            });
        }
        
        /// <summary>
        /// カウンター攻撃データを取得（デフォルトのカウンター攻撃を返す）
        /// </summary>
        private AttackData GetCounterAttackData()
        {
            // InGameManagerのテスト機能ではnextChainを使用するが、
            // 現在は直接アクセスできないので、nullを返す
            // 後でInGameManagerにパブリックメソッドを追加するか、
            // またはキャラクターのデフォルトカウンター攻撃を使用する
            return null;
        }
        
        /// <summary>
        /// カウンター攻撃を非同期で実行（InGameManagerのロジックと同様）
        /// </summary>
        private async UniTaskVoid ExecuteCounterAttackAsync(AttackData attackData)
        {
            // 数フレーム待機して、現在の処理が完全に終了するのを待つ
            await UniTask.DelayFrame(3, PlayerLoopTiming.Update);
            
            // 攻撃可能な状態か確認
            if (controller != null && controller.StateMachine != null)
            {
                // 現在の攻撃をキャンセル
                controller.Anim.SetBool(AnimatorParam.IsAttacking, false);
                
                // カウンター攻撃フラグを立てる
                controller.SetExecutingCounterAttack(true);
                
                // 一旦ニュートラル状態に戻してから攻撃を実行（InGameManagerと同じ方式）
                controller.StateMachine.ChangeState(controller.GetState<NeutralState>());
                
                // さらに数フレーム待機してステート切り替えを確実にする
                await UniTask.DelayFrame(2, PlayerLoopTiming.Update);
                
                // 攻撃を実行
                controller.Attack(AttackType.Weak, AttackDirection.Neutral);
                
                if (attackData != null)
                {
                    // Debug.Log($"Player {controller.PlayerIndex} カウンター攻撃実行: {attackData.attackName}");
                }
                else
                {
                    // Debug.Log($"Player {controller.PlayerIndex} カウンター攻撃実行: デフォルト攻撃");
                }
                
                // カウンター攻撃終了後にフラグをクリア
                await UniTask.DelayFrame(30, PlayerLoopTiming.Update); // 攻撃アニメーション完了を待つ
                if (controller != null)
                {
                    controller.SetExecutingCounterAttack(false);
                }
            }
        }

        /// <summary>
        /// 攻撃判定の実行,判定は調停者に任せる
        /// </summary>
        /// <returns></returns>
        private void PerformHitDetection()
        {
            try
            {
                Vector3 hitPosition = controller.transform.position +
                    controller.transform.TransformDirection(currentAttackData.hitboxOffset);

                Collider[] hits = Physics.OverlapSphere(
                    hitPosition,
                    currentAttackData.radius,
                    currentAttackData.targetLayers
                );
                AttackVisualizer.I.DrawHitbox(hitPosition, currentAttackData.radius);

                // BattleJudgeに判定を依頼
                InGameManager.I.BattleBus.Publish(new AttackRequestEvent
                {
                    attacker = controller,
                    attackData = currentAttackData,
                    hitPosition = hitPosition,
                    hitTargets = hits
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"PerformHitDetection failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 攻撃Prefabの生成
        /// </summary>
        private void CreateAttackObject()
        {
            if (currentAttackData.attackPrefab == null) return;
            Vector3 spawnPos = controller.transform.position +
                controller.transform.TransformDirection(currentAttackData.prefabOffset);
            Quaternion spawnRot = controller.transform.rotation *
                Quaternion.Euler(currentAttackData.prefabRotation);
            // 攻撃オブジェクトを取得
            var obj = CharaAttackFactory.I.GetAttackObj(currentAttackData.attackPrefab, spawnPos, spawnRot);
            obj.GetComponent<AttackObjectController>()?.SetPlayer(controller.PlayerIndex, controller.gameObject);
        }
    }
}