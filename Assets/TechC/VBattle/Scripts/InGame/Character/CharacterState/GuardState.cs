using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    // ==========================================
    // Guard状態（ガード中）
    // ==========================================
    public class GuardState : CharacterState
    {
        public GuardState(CharacterController controller) : base(controller) { }

        public override bool CanExecuteCommand<T>(T command)
        {
            // ガード中はガード解除のみ
            return command.Type == CommandType.Guard;
        }

        public override void OnEnter(CharacterState prevState)
        {
            Debug.Log("Enter Guard");
            // ガードエフェクト表示など
        }

        public override async UniTask<CharacterState> OnUpdate(CancellationToken ct)
        {
            // ガードボタンが離されるまで待機
            while (!ct.IsCancellationRequested)
            {
                await UniTask.Yield(ct);
            }
            return this;
        }

        public override void OnExit()
        {
            Debug.Log("Exit Guard");
        }
    }
}