using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Util;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// ダメージ状態
    /// </summary>
    public class DamageState : CharacterState
    {
        private float damageStunDuration = 0.3f;
        private AttackData attackData;
        private Vector3 attackerPosition; // 変更: 攻撃者位置を保持
        private float knockbackForce;
        private Vector3 knockbackDirection;
        private bool hasKnockback = false;
        private bool isWallBouncing = false; // 壁バウンス中かどうか
        private float wallBounceElapsedTime = 0f; // 壁バウンス経過時間
        private int wallHitDirection = 0; // 壁に当たった方向（1=前方, -1=後方）
        public override int Priority => 10;
        private bool hasWallBounced = false;
        public DamageState(CharacterController controller) : base(controller) { }

        public override bool CanExecuteCommand<T>(T command)
        {
            return false;
        }

        public override void OnEnter(CharacterState prevState)
        {
            controller.Anim.SetBool(AnimatorParam.IsHitting, false);
            controller.Anim.SetBool(AnimatorParam.IsWallHitting, false);
            isWallBouncing = false;
            wallBounceElapsedTime = 0f;
            hasWallBounced = false;

            // 通常のダメージアニメーション
            AnimatorUtil.SetAnimatorBoolExclusive(controller.Anim, AnimatorParam.IsHitting);
            ApplyKnockback();
        }

        public override async UniTask<CharacterState> OnUpdate(CancellationToken ct)
        {
            float elapsedTime = 0f;

            while (elapsedTime < damageStunDuration)
            {
                // 壁バウンド判定（まだ壁バウンスしていない場合のみ）
                if (!hasWallBounced && attackData != null && attackData.causesWallBounce)
                {

                    if (CheckWallBehind(out int hitDir))
                    {
                        // 壁バウンド開始
                        wallHitDirection = hitDir;
                        StartWallBounce();
                        elapsedTime = 0f; // 時間をリセット
                        continue;
                    }
                }

                // 壁バウンス中の時間カウント
                if (isWallBouncing)
                {
                    wallBounceElapsedTime += Time.deltaTime;
                }
                await UniTask.Yield(ct);
                elapsedTime += Time.deltaTime;
            }

            if (!controller.IsGrounded())
                return controller.GetState<AirState>();

            return controller.GetState<NeutralState>();
        }

        public override void OnExit()
        {
            controller.Anim.SetBool(AnimatorParam.IsHitting, false);
            controller.Anim.SetBool(AnimatorParam.IsWallHitting, false);
            hasKnockback = false;
            knockbackForce = 0f;
            attackerPosition = Vector3.zero;
            isWallBouncing = false;
            wallBounceElapsedTime = 0f;
            wallHitDirection = 0;
        }

        /// <summary>
        /// AttackDataと攻撃者位置から情報を設定
        /// </summary>
        public void SetDamageInfo(AttackData data, Vector3 attackerPos)
        {
            attackData = data;
            attackerPosition = attackerPos;

            if (data == null) return;
            damageStunDuration = data.hitStunDuration;

            if (data.knockbackForce > 0f)
            {
                knockbackForce = data.knockbackForce;
                knockbackDirection = data.knockbackDirection;
                hasKnockback = true;
            }
        }

        /// <summary>
        /// スタン時間のみ設定
        /// </summary>
        public void SetStunDuration(float duration)
        {
            damageStunDuration = duration;
        }

        /// <summary>
        /// ノックバック情報を設定（攻撃者位置ベース）
        /// </summary>
        public void SetKnockback(Vector3 attackerPos, float force, Vector3 dir)
        {
            attackerPosition = attackerPos;
            knockbackForce = force;
            knockbackDirection = dir;
            hasKnockback = knockbackForce > 0.01f;
        }

        /// <summary>
        /// 攻撃者位置とknockbackDirectionからノックバック適用
        /// </summary>
        private void ApplyKnockback()
        {
            if (!hasKnockback || knockbackForce <= 0f) return;

            var rb = controller.Rb;
            if (rb == null) return;

            // 攻撃者から被ダメージ者への方向ベクトル
            Vector3 fromAttacker = (controller.transform.position - attackerPosition).normalized;

            // knockbackDirectionを攻撃者基準の方向に変換
            Vector3 finalDirection = fromAttacker.x * knockbackDirection.x * Vector3.right +
                                    knockbackDirection.y * Vector3.up +
                                    fromAttacker.z * knockbackDirection.z * Vector3.forward;

            finalDirection = finalDirection.normalized;

            // ノックバック適用
            rb.velocity = finalDirection * knockbackForce;

            // CustomLogger.Info($"Knockback: attackerPos={attackerPosition}, dir={finalDirection}, force={knockbackForce}", LogTagUtil.TagState);
        }

        /// <summary>
        /// 前方または後方に壁があるかチェック
        /// </summary>
        /// <param name="hitDirection">壁に当たった方向（out: 1=前方, -1=後方, 0=壁なし）</param>
        /// <returns>壁に当たったかどうか</returns>
        private bool CheckWallBehind(out int hitDirection)
        {
            hitDirection = 0;
            var wallCheckDistance = 0.6f;
            Vector3 origin = controller.transform.position + Vector3.up * 0.5f; // 腰あたりの高さ

            // 後方にレイキャスト
            Vector3 backward = -controller.transform.forward;
            bool hitWallBack = Physics.Raycast(origin, backward, out RaycastHit hitBack, wallCheckDistance, LayerMask.GetMask("Wall"));

            // 前方にレイキャスト
            Vector3 forward = controller.transform.forward;
            bool hitWallForward = Physics.Raycast(origin, forward, out RaycastHit hitForward, wallCheckDistance, LayerMask.GetMask("Wall"));

            // デバッグ描画（常に表示、色を分ける）
            Debug.DrawRay(origin, backward * wallCheckDistance, hitWallBack ? Color.red : Color.yellow, 2.0f);
            Debug.DrawRay(origin, forward * wallCheckDistance, hitWallForward ? Color.green : Color.cyan, 2.0f);

            // ログ出力
            if (hitWallBack || hitWallForward)
            {
                CustomLogger.Info($"Wall Check: Forward={hitWallForward}, Back={hitWallBack}", LogTagUtil.TagState);
            }

            // 壁に当たった方向を設定
            if (hitWallForward)
            {
                hitDirection = 1; // 前方の壁に当たった
                CustomLogger.Info($"Hit FORWARD wall, will bounce BACKWARD", LogTagUtil.TagState);
                return true;
            }
            else if (hitWallBack)
            {
                hitDirection = -1; // 後方の壁に当たった
                CustomLogger.Info($"Hit BACKWARD wall, will bounce FORWARD", LogTagUtil.TagState);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 壁バウンド開始
        /// </summary>
        private void StartWallBounce()
        {
            if (attackData == null || !attackData.causesWallBounce) return;

            isWallBouncing = true;
            wallBounceElapsedTime = 0f;

            // 壁バウンド専用アニメーションに切り替え
            controller.Anim.SetBool(AnimatorParam.IsHitting, false);
            AnimatorUtil.SetAnimatorBoolExclusive(controller.Anim, AnimatorParam.IsWallHitting);

            // 壁バウンド時間を適用
            damageStunDuration = attackData.wallBounceTime;

            // 壁から跳ね返る速度を適用
            var rb = controller.Rb;
            if (rb != null)
            {
                // 壁に当たった方向の反対方向に跳ね返す
                // wallHitDirection: 1=前方の壁に当たった→後方(-forward)に跳ね返す
                //                  -1=後方の壁に当たった→前方(+forward)に跳ね返す
                Vector3 bounceDirection = -wallHitDirection * controller.transform.forward;
                bounceDirection.y = 0; // 水平方向のみ
                bounceDirection = bounceDirection.normalized;

                // 上方向のブーストを追加
                Vector3 bounceVelocity = bounceDirection * attackData.wallBounceForce +
                                        Vector3.up * attackData.wallBounceVerticalBoost;

                rb.velocity = bounceVelocity;

                CustomLogger.Info($"Wall Bounce START: hitDir={wallHitDirection}, bounceDir={bounceDirection}, velocity={bounceVelocity}, force={attackData.wallBounceForce}, boost={attackData.wallBounceVerticalBoost}", LogTagUtil.TagState);
                Debug.Log($"Wall Bounce: hitDir={wallHitDirection}, bounceDir={bounceDirection}, force={attackData.wallBounceForce}");
            }
            hasWallBounced = true;
        }
    }
}