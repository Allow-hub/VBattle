using System.Threading;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Util;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 被ダメージステート
    /// </summary>
    public class DamageState : CharacterState
    {
        private float damageStunDuration = 0.3f;

        public DamageState(CharacterController controller) : base(controller) { }

        public override bool CanExecuteCommand<T>(T command)
        {
            // ダメージ中は一切の行動不可
            return false;
        }

        public override void OnEnter(CharacterState prevState)
        {
            Debug.Log("Enter Damage");
            AnimatorUtil.SetAnimatorBoolExclusive(controller.Anim,AnimatorParam.IsHitting);
            // ノックバック処理など
        }

        public override async UniTask<CharacterState> OnUpdate(CancellationToken ct)
        {
            // ダメージ硬直時間待機
            await UniTask.Delay((int)(damageStunDuration * 1000), cancellationToken: ct);

            // 空中でダメージを受けた場合
            if (!controller.IsGrounded())
            {
                return controller.GetState<AirState>();
            }

            // 地上なら通常状態へ
            return controller.GetState<NeutralState>();
        }

        public override void OnExit()
        {
            Debug.Log("Exit Damage");
        }

        public void SetStunDuration(float duration)
        {
            damageStunDuration = duration;
        }
    }
}
