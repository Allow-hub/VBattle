using System;
using UnityEngine;

namespace Allow
{
    /// <summary>
    /// リスト化したC#クラスのふるまいの例
    /// このクラスではライフタイムを設定する
    /// </summary>
    [Serializable]
    public class ExampleLifeTime : IExampleBehaviour
    {
        [SerializeField] private float lifeTime;
        private float currentLifeTime;
        private GameObject ownerObj;
        public void Initialize(GameObject owner)
        {
            currentLifeTime = lifeTime;
            ownerObj = owner;
        }

        public void OnRelease()
        {
            currentLifeTime = lifeTime;
        }

        public void OnUpdate(float deltaTime)
        {
            if (currentLifeTime <= 0)
            {
                currentLifeTime = 0;
                ownerObj.SetActive(false);
                return;
            }
            currentLifeTime -= deltaTime;
        }
    }
}
