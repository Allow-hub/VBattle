using System;
using System.Collections.Generic;

namespace TechC.VBattle.InGame.Systems
{
    public class BattleEventBus
    {
        private readonly Dictionary<Type, Delegate> eventTable = new();

        // Publish
        public void Publish<T>(T e)
        {
            if (eventTable.TryGetValue(typeof(T), out var del))
            {
                (del as Action<T>)?.Invoke(e);
            }
        }

        // Subscribe
        public void Subscribe<T>(Action<T> listener)
        {
            if (eventTable.TryGetValue(typeof(T), out var del))
                eventTable[typeof(T)] = Delegate.Combine(del, listener);
            else
                eventTable[typeof(T)] = listener;
        }

        // Unsubscribe
        public void Unsubscribe<T>(Action<T> listener)
        {
            if (eventTable.TryGetValue(typeof(T), out var del))
            {
                var newDel = Delegate.Remove(del, listener);
                if (newDel == null)
                    eventTable.Remove(typeof(T));
                else
                    eventTable[typeof(T)] = newDel;
            }
        }

        public void ClearAllListeners() => eventTable.Clear();
    }
}
