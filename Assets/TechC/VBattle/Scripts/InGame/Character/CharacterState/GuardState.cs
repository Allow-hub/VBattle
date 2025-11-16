using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    public class GuardState : CharacterState
    {
        public GuardState(CharacterController controller) : base(controller) { }

        public override bool CanExecuteCommand<T>(T command)
        {
            // ガード中に許可するのは「ガード解除」だけ
            return command.Type == CommandType.Guard;
        }

        public override void OnEnter(CharacterState prevState)
        {
        }

        public override async UniTask<CharacterState> OnUpdate(CancellationToken ct)
        {
            var data = controller.Data;

            // --- ガード中の耐久値減少 ---
            controller.DecreaseGuardPower(data.GuardDecreasePower * Time.deltaTime);
            Debug.Log(controller.CurrentGuardPower);

            // ガードブレイク判定
            if (controller.CurrentGuardPower <= 0f)
            {
                controller.SetGuardPower(0f);

                // ガードブレイク状態へ遷移（DamageState など）
                return controller.GetState<DamageState>();
            }

            // ボタンが離されたらガード終了（Invoker が GuardCommand(false) を発行）
            if (!controller.IsGuarding)
            {
                // NeutralState に戻る
                return controller.GetState<NeutralState>();
            }
            try
            {
                await UniTask.Yield(ct);
            }
            catch (System.OperationCanceledException)
            {
                // 外部からChangeStateされた場合（攻撃を受けてダメージ状態になるなど）
                throw; // 再スローしてStateMachineに伝える
            }
            return this;
        }

        public override void OnExit()
        {
        }
    }
}