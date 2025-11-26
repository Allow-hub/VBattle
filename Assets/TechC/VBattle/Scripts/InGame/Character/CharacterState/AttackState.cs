using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Util;

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
            // キャンセル可能タイミングでのみ次の攻撃を受け付ける
            if (command.Type == CommandType.Attack && canCancel)
            {
                // 連鎖攻撃が可能かチェック
                if (currentAttackData.canChain && currentAttackData.nextChain != null)
                {
                    // 連鎖攻撃をリクエスト
                    isChainRequested = true;
                    return true;
                }
            }
            return false;
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
            // 攻撃ループ（連鎖攻撃対応）
            while (true)
            {
                isChainRequested = false;

                float attackTime = currentAttackData.attackDuration;
                float recoveryTime = currentAttackData.recoveryDuration;

                // 攻撃開始（キャンセル不可）
                canCancel = false;
                await UniTask.Delay(TimeSpan.FromSeconds(currentAttackData.cancelStartTime), cancellationToken: ct);

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
    }
}