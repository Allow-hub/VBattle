using System;
using System.Collections.Generic;

namespace TechC.VBattle.Select.Events
{
    /// <summary>
    /// Select画面で発生するイベントを Pub/Sub 方式で管理するイベントバス
    /// 各イベント型ごとにリスナーを登録し、任意のタイミングでイベントを発行
    /// 所有者はSelectUIManager
    /// </summary>
    public class SelectEventBus
    {
        /// <summary>
        /// イベント型ごとのデリゲートディクショナリー
        /// key: イベントの型 (typeof(T))
        /// value: 登録されたデリゲート
        /// </summary>
        private Dictionary<Type, Delegate> eventDictionary = new();

        /// <summary>
        /// 指定したイベント型に対してリスナーを登録する。
        /// 同じイベント型に複数のリスナーを追加登録できる。
        /// </summary>
        /// <typeparam name="T">購読するイベント型。ISelectEvent を実装している必要がある。</typeparam>
        /// <param name="listener">イベント発行時に呼び出されるコールバック。</param>
        public void Subscribe<T>(Action<T> listener) where T : ISelectEvent
        {
            var eventType = typeof(T);

            if (eventDictionary.TryGetValue(eventType, out var existingDelegate))
                eventDictionary[eventType] = Delegate.Combine(existingDelegate, listener);
            else
                eventDictionary[eventType] = listener;
        }

        /// <summary>
        /// 指定したイベント型からリスナーを解除
        /// 該当リスナーが最後の1つの場合はイベント辞書からエントリごと削除される
        /// </summary>
        /// <typeparam name="T">解除対象のイベント型</typeparam>
        /// <param name="listener">登録解除するリスナー</param>
        public void Unsubscribe<T>(Action<T> listener) where T : ISelectEvent
        {
            var eventType = typeof(T);

            if (eventDictionary.TryGetValue(eventType, out var existingDelegate))
            {
                var newDelegate = Delegate.Remove(existingDelegate, listener);

                if (newDelegate == null)
                    eventDictionary.Remove(eventType);
                else
                    eventDictionary[eventType] = newDelegate;
            }
        }

        /// <summary>
        /// 指定したイベントをすべての登録リスナーに対して発行
        /// </summary>
        public void Publish<T>(T eventData) where T : ISelectEvent
        {
            var eventType = typeof(T);
            if (eventDictionary.TryGetValue(eventType, out var existingDelegate))
                (existingDelegate as Action<T>)?.Invoke(eventData);
        }

        public void Clear() => eventDictionary.Clear();
    }
}
