using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Core;
using TechC.VBattle.Systems;
using UnityEngine;

namespace TechC.VBattle.InGame.Gimmick
{
    /// <summary>
    /// 壁にぶつかった時に破片を飛び散らかす処理
    /// 複数回の爆発と最適化を追加
    /// </summary>
    public class ExplosionDebris : MonoBehaviour
    {
        [Serializable]
        public struct TransformSnapshot
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;

            public TransformSnapshot(Transform t)
            {
                position = t.localPosition;
                rotation = t.localRotation;
                scale = t.localScale;
            }

            public void ApplyTo(Transform t)
            {
                t.localPosition = position;
                t.localRotation = rotation;
                t.localScale = scale;
            }
        }

        [SerializeField] private GameObject parentObj;
        [SerializeField] float lifeTime = 2f;
        private CancellationTokenSource lifeCts;

        [SerializeField, ReadOnly] private List<GameObject> childObj = new List<GameObject>();
        [SerializeField, ReadOnly] private List<Transform> childTrans = new List<Transform>();
        [SerializeField, ReadOnly] private List<TransformSnapshot> originalTransforms = new List<TransformSnapshot>();

        // Rigidbodyのキャッシュ用リスト
        [SerializeField, ReadOnly] private List<Rigidbody> childRigidbodies = new List<Rigidbody>();

        [SerializeField, Range(0f, 200f)] private float explosionForceMin = 100f;
        [SerializeField, Range(0f, 200f)] private float explosionForceMax = 500f;
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private float upwardsModifier = 0.5f;

        // 爆発の状態を追跡するフラグ
        private bool hasExploded = false;

        private void OnValidate()
        {
            if (parentObj == null) return;

            childObj.Clear();
            childTrans.Clear();
            originalTransforms.Clear();
            childRigidbodies.Clear();

            int childCount = parentObj.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = parentObj.transform.GetChild(i);
                childObj.Add(child.gameObject);
                childTrans.Add(child);
                originalTransforms.Add(new TransformSnapshot(child));

                // Rigidbodyをキャッシュ
                Rigidbody rb = child.GetComponent<Rigidbody>();
                if (rb == null) rb = child.gameObject.AddComponent<Rigidbody>();
                childRigidbodies.Add(rb);
            }
        }

        private void OnEnable()
        {
            Explode();
            StartLifetime().Forget();
        }

        private void OnDisable()
        {
            lifeCts?.Cancel();
            lifeCts?.Dispose();
            lifeCts = null;

            ResetExplosion();
        }

        /// <summary>
        /// 明示的に爆発を呼び出すメソッド
        /// </summary>
        public void Explode()
        {
            Vector3 explosionPosition = transform.position;

            for (int i = 0; i < childObj.Count; i++)
            {
                GameObject obj = childObj[i];
                Transform t = childTrans[i];
                TransformSnapshot snapshot = originalTransforms[i];
                Rigidbody rb = childRigidbodies[i];

                // Transformを初期状態に戻す
                snapshot.ApplyTo(t);

                // 物理状態をリセット
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // キネマティックモードを解除
                rb.isKinematic = false;

                // ランダムな回転を適用
                t.rotation = UnityEngine.Random.rotation;

                // 爆発力を加える
                float randomForce = UnityEngine.Random.Range(explosionForceMin, explosionForceMax);
                rb.AddExplosionForce(randomForce, explosionPosition, explosionRadius, upwardsModifier, ForceMode.Impulse);
            }

            hasExploded = true;
        }

        /// <summary>
        /// 明示的に爆発をリセットするメソッド
        /// </summary>
        public void ResetExplosion()
        {
            if (!hasExploded) return;

            for (int i = 0; i < childObj.Count; i++)
            {
                Transform t = childTrans[i];
                TransformSnapshot snapshot = originalTransforms[i];
                Rigidbody rb = childRigidbodies[i];

                // 元の状態に戻す
                snapshot.ApplyTo(t);
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            hasExploded = false;
        }
        private async UniTaskVoid StartLifetime()
        {
            lifeCts?.Cancel();
            lifeCts?.Dispose();

            lifeCts = new CancellationTokenSource();

            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(lifeTime),
                    cancellationToken: lifeCts.Token
                );

                EffectFactory.I?.ReturnEffect(gameObject);
            }
            catch (OperationCanceledException)
            {
                // 無視
            }
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // 半透明のオレンジ
            Gizmos.DrawSphere(transform.position, explosionRadius);
        }
    }
}