using System.Collections.Generic;
using UnityEngine;

namespace Allow
{
    /// <summary>
    /// ピュアなC#クラスのリストを扱うクラス
    /// </summary>
    public class ExampleController : MonoBehaviour
    {
        [SerializeReference] private List<IExampleBehaviour> behaviours;

        private void Start()
        {
            if (behaviours == null) return;

            foreach (var behaviour in behaviours)
            {
                if (behaviour == null) continue;
                behaviour?.Initialize(gameObject);
            }
        }

        private void OnDisable()
        {
            if (behaviours == null) return;

            foreach (var behaviour in behaviours)
            {
                if (behaviour == null) continue;
                behaviour?.OnRelease();
            }
        }

        private void Update()
        {
            if (behaviours == null) return;

            float delta = Time.deltaTime;
            foreach (var behaviour in behaviours)
            {
                if (behaviour == null) continue;
                behaviour?.OnUpdate(delta);
            }
        }
    }
}