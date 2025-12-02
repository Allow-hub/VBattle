using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC.VBattle.Core.Util
{
    /// <summary>
    /// UniTaskを使用した遅延処理、ポーズに対応する
    /// IEnumeratorは事前に生成してキャッシュしたほうが生成コストを抑えられるらしい 
    ///  UniTaskは値型なので気にする必要ない 
    ///  </summary>
    public static class DelayUtility
    {
        // ================================
        // 単発遅延：非ポーズ対応
        // ================================

        /// <summary>
        /// 指定秒数後にコールバックを実行（非ポーズ対応）
        /// </summary>
        public static async UniTask RunAfterDelay(float delaySeconds, Action callback, CancellationToken token = default)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken: token);
            callback?.Invoke();
        }

        /// <summary>
        /// 指定秒数後にコールバックを実行（非ポーズ対応）のラッパー
        /// </summary>
        public static UniTask StartDelayedActionAsync(float delaySeconds, Action callback, CancellationToken token = default)
        {
            return RunAfterDelay(delaySeconds, callback, token);
        }

        // ================================
        // 単発遅延：ポーズ対応
        // ================================

        /// <summary>
        /// 指定秒数後にコールバックを実行、ポーズ中は進めない
        /// </summary>
        public static async UniTask RunAfterDelayWithPause(float delaySeconds, Action callback, Func<bool> isPausedFunc, CancellationToken token = default)
        {
            float elapsed = 0f;

            while (elapsed < delaySeconds)
            {
                token.ThrowIfCancellationRequested();

                if (isPausedFunc != null && isPausedFunc())
                {
                    await UniTask.Yield();
                    continue;
                }

                elapsed += Time.deltaTime;
                await UniTask.Yield();
            }

            callback?.Invoke();
        }

        /// <summary>
        /// 指定秒数後にコールバックを実行、ポーズ中は進めないラッパー
        /// </summary>
        public static UniTask StartDelayedActionWithPauseAsync(float delaySeconds, Action callback, Func<bool> isPausedFunc, CancellationToken token = default)
        {
            return RunAfterDelayWithPause(delaySeconds, callback, isPausedFunc, token);
        }

        // ================================
        // 一定間隔で繰り返し実行：非ポーズ対応
        // ================================

        /// <summary>
        /// 指定時間の間、一定間隔で非同期コールバックを繰り返し実行
        /// </summary>
        public static async UniTask RunRepeatedly(float duration, float interval, Func<UniTask> callback, CancellationToken token = default)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();

                if (callback != null)
                    await callback();

                float t = 0f;
                while (t < interval)
                {
                    token.ThrowIfCancellationRequested();
                    await UniTask.Yield();
                    t += Time.deltaTime;
                    elapsed += Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// 指定時間の間、一定間隔で非同期コールバックを繰り返し実行するラッパー
        /// </summary>
        public static UniTask StartRepeatedActionAsync(float duration, float interval, Func<UniTask> callback, CancellationToken token = default)
        {
            return RunRepeatedly(duration, interval, callback, token);
        }

        // ================================
        // 一定間隔で繰り返し実行：ポーズ対応
        // ================================

        /// <summary>
        /// 指定時間の間、一定間隔で非同期コールバックを繰り返し実行
        /// ポーズ中は処理を停止
        /// </summary>
        public static async UniTask RunRepeatedlyWithPause(float duration, float interval, Func<UniTask> callback, Func<bool> isPausedFunc, CancellationToken token = default)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();

                while (isPausedFunc?.Invoke() == true)
                {
                    token.ThrowIfCancellationRequested();
                    await UniTask.Yield();
                }

                if (callback != null)
                    await callback();

                float t = 0f;
                while (t < interval)
                {
                    token.ThrowIfCancellationRequested();

                    while (isPausedFunc?.Invoke() == true)
                    {
                        token.ThrowIfCancellationRequested();
                        await UniTask.Yield();
                    }

                    await UniTask.Yield();
                    t += Time.deltaTime;
                    elapsed += Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// 指定時間の間、一定間隔で非同期コールバックを繰り返し実行（ポーズ対応）のラッパー
        /// </summary>
        public static UniTask StartRepeatedActionWithPauseAsync(float duration, float interval, Func<UniTask> callback, Func<bool> isPausedFunc, CancellationToken token = default)
        {
            return RunRepeatedlyWithPause(duration, interval, callback, isPausedFunc, token);
        }

        // ================================
        // 条件付き繰り返し：非ポーズ対応
        // ================================

        /// <summary>
        /// 条件が true の間、一定間隔で非同期コールバックを繰り返し実行
        /// </summary>
        public static async UniTask RunRepeatedlyWhile(Func<bool> shouldContinueFunc, float interval, Func<UniTask> callback, CancellationToken token = default)
        {
            while (shouldContinueFunc == null || shouldContinueFunc())
            {
                token.ThrowIfCancellationRequested();
                if (callback != null)
                    await callback();

                float t = 0f;
                while (t < interval)
                {
                    token.ThrowIfCancellationRequested();
                    await UniTask.Yield();
                    t += Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// 条件が true の間、一定間隔で非同期コールバックを繰り返し実行するラッパー
        /// </summary>
        public static UniTask StartRepeatedActionWhileAsync(Func<bool> shouldContinueFunc, float interval, Func<UniTask> callback, CancellationToken token = default)
        {
            return RunRepeatedlyWhile(shouldContinueFunc, interval, callback, token);
        }

        // ================================
        // 条件付き繰り返し：ポーズ対応
        // ================================

        /// <summary>
        /// 条件が true の間、一定間隔で非同期コールバックを繰り返し実行
        /// ポーズ中は処理を停止
        /// </summary>
        public static async UniTask RunRepeatedlyWhileWithPause(Func<bool> shouldContinueFunc, float interval, Func<UniTask> callback, Func<bool> isPausedFunc, CancellationToken token = default)
        {
            while (shouldContinueFunc == null || shouldContinueFunc())
            {
                token.ThrowIfCancellationRequested();

                while (isPausedFunc?.Invoke() == true)
                {
                    token.ThrowIfCancellationRequested();
                    await UniTask.Yield();
                }

                if (callback != null)
                    await callback();

                float t = 0f;
                while (t < interval)
                {
                    token.ThrowIfCancellationRequested();

                    while (isPausedFunc?.Invoke() == true)
                    {
                        token.ThrowIfCancellationRequested();
                        await UniTask.Yield();
                    }

                    await UniTask.Yield();
                    t += Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// 条件が true の間、一定間隔で非同期コールバックを繰り返し実行（ポーズ対応）
        /// </summary>
        public static UniTask StartRepeatedActionWhileWithPauseAsync(Func<bool> shouldContinueFunc, float interval, Func<UniTask> callback, Func<bool> isPausedFunc, CancellationToken token = default)
        {
            return RunRepeatedlyWhileWithPause(shouldContinueFunc, interval, callback, isPausedFunc, token);
        }
    }
}