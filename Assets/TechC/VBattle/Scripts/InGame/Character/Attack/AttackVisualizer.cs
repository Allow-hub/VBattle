using System.Collections.Generic;
using System.Diagnostics;
using TechC.VBattle.Core.Managers;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 攻撃判定のデバッグ表示をするクラス
    /// </summary>
    public class AttackVisualizer : Singleton<AttackVisualizer>
    {
        [System.Serializable]
        public class HitboxInfo
        {
            public Vector3 position;
            public float radius;
            public Color color;
            public float duration;
            public float startTime;
            public bool IsExpired => Time.time > startTime + duration;
        }

        protected override bool UseDontDestroyOnLoad => false;

        // 使い回すインスタンス
        private readonly Stack<HitboxInfo> pool = new();

        // 描画中のヒットボックス
        // 拡張のために最初からある程度の容量を確保
        private readonly List<HitboxInfo> activeHitboxes = new(32);

        // デフォルトカラー
        private static readonly Color defaultColor = new(1f, 0f, 0f, 0.5f);

        public override void Init()
        {
            base.Init();
        }

        /// <summary>
        /// ヒットボックスの表示をリクエスト
        /// </summary
        [Conditional("UNITY_EDITOR")]
        public void DrawHitbox(Vector3 position, float radius, float duration = 0.5f, Color? color = null)
        {
            // プールから取得 or 生成
            var info = pool.Count > 0 ? pool.Pop() : new HitboxInfo();

            info.position = position;
            info.radius = radius;
            info.color = color ?? defaultColor;
            info.duration = duration;
            info.startTime = Time.time;

            activeHitboxes.Add(info);
        }

#if UNITY_EDITOR
        private void LateUpdate()
        {
            //消えたヒットボックスを回収
            for (int i = activeHitboxes.Count - 1; i >= 0; i--)
            {
                if (activeHitboxes[i].IsExpired)
                {
                    pool.Push(activeHitboxes[i]);
                    activeHitboxes.RemoveAt(i);
                }
            }
        }
#endif

        private void OnDrawGizmos()
        {
            for (int i = 0; i < activeHitboxes.Count; i++)
            {
                var h = activeHitboxes[i];
                Gizmos.color = h.color;
                Gizmos.DrawSphere(h.position, h.radius);
            }
        }
    }
}