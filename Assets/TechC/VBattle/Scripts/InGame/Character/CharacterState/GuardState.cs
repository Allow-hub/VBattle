using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// ガード状態
    /// </summary>
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

            while (!ct.IsCancellationRequested)
            {
                // ガード中の耐久値減少
                var deltaTime = Time.deltaTime;
                if (deltaTime > 0f)
                {
                    controller.DecreaseGuardPower(data.GuardDecreasePower * deltaTime);
                }

                // ガードブレイク判定
                if (controller.CurrentGuardPower <= 0f)
                {
                    controller.SetGuardPower(0f);
                    controller.EndGuard();
                    return controller.GetState<DamageState>();
                }

                // ボタンが離されたらガード終了
                if (!controller.IsGuarding)
                {
                    return controller.GetState<NeutralState>();
                }
                // 次のフレームまで待機
                await UniTask.Yield(ct);
            }
            
            return this;
        }

        public override void OnExit()
        {
        }
    }
}