using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        private AttackData currentAttackData;
        private bool canCancel = false;
        private bool isAirAttack = false;
        private bool isChainRequested = false;
        private int chain = 0;

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
            // 攻撃ループ
            while (true)
            {
                isChainRequested = false;

                float attackTime = currentAttackData.attackDuration;
                float recoveryTime = currentAttackData.recoveryDuration;

                // 攻撃開始（キャンセル不可）
                canCancel = false;

                // hitTimingまで待機
                await UniTask.Delay(TimeSpan.FromSeconds(currentAttackData.hitTiming), cancellationToken: ct);

                // 攻撃Prefab生成と判定を実行
                CreateAttackObject();
                PerformHitDetection();

                // cancelStartTimeまでの残り時間を待機
                float remainingToCancelStart = currentAttackData.cancelStartTime - currentAttackData.hitTiming;
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
        }

        /// <summary>
        /// 攻撃判定の実行,判定は調停者に任せる
        /// </summary>
        /// <returns></returns>
        private void PerformHitDetection()
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
        
        /// <summary>
        /// 攻撃Prefabの生成
        /// </summary>
        private void CreateAttackObject()
        {
            if (currentAttackData.attackPrefab != null)
            {
                Vector3 spawnPos = controller.transform.position +
                    controller.transform.TransformDirection(currentAttackData.prefabOffset);
                Quaternion spawnRot = controller.transform.rotation *
                    Quaternion.Euler(currentAttackData.prefabRotation);
                // 攻撃オブジェクトを取得
                CharaAttackFactory.I.GetAttackObj(currentAttackData.attackPrefab, spawnPos, spawnRot);
            }
        }
    }
}