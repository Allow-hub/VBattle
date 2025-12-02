using System;
using Cysharp.Threading.Tasks;
using TechC.VBattle.InGame.Events;
using UnityEngine;

namespace TechC.VBattle.InGame.Systems
{
    /// <summary>
    /// ヒットストップを管理するクラス
    /// TimeScaleを補間しながら変更する
    /// </summary>
    public class HitStopManager
    {
        private readonly BattleEventBus eventBus;
        private bool isHitStopping = false;

        // 補間設定
        private const float INTERPOLATION_DURATION = 0.05f; // 補間にかける時間（秒）

        public HitStopManager(BattleEventBus eventBus)
        {
            this.eventBus = eventBus;
            eventBus.Subscribe<AttackResultEvent>(OnAttackResult);
        }

        private async void OnAttackResult(AttackResultEvent e)
        {
            if (!e.isHit) return;
            if (e.attackData.hitStopDuration <= 0f) return;

            await ApplyHitStop(e);
        }

        private async UniTask ApplyHitStop(AttackResultEvent e)
        {
            if (isHitStopping) return; // 多重発動防止

            isHitStopping = true;

            float originalTimeScale = Time.timeScale;
            float targetTimeScale = e.attackData.hitStopTimeScale;
            float hitStopDuration = e.attackData.hitStopDuration;

            // TimeScaleを補間しながら減速
            await InterpolateTimeScale(originalTimeScale, targetTimeScale, INTERPOLATION_DURATION);

            // ヒットストップの持続時間を待機（補間時間を引く）
            float remainingDuration = Mathf.Max(0, hitStopDuration - INTERPOLATION_DURATION);
            if (remainingDuration > 0)
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(remainingDuration),
                    ignoreTimeScale: true
                );
            }

            // TimeScaleを補間しながら元に戻す
            await InterpolateTimeScale(targetTimeScale, originalTimeScale, INTERPOLATION_DURATION);

            isHitStopping = false;
        }

        /// <summary>
        /// TimeScaleを補間する
        /// </summary>
        private async UniTask InterpolateTimeScale(float from, float to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime; // 実時間で進める
                float t = Mathf.Clamp01(elapsed / duration);
                
                // イージング（オプション：EaseOutQuadで滑らかに）
                float easedT = 1f - (1f - t) * (1f - t);
                
                Time.timeScale = Mathf.Lerp(from, to, easedT);
                
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            // 最終値を確実に設定
            Time.timeScale = to;
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<AttackResultEvent>(OnAttackResult);
        }
    }
}