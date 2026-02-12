using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// Transformの動きを記録・再生するコンポーネント
    /// </summary>
    public class TransformRecorder : MonoBehaviour
    {
        public List<TransformData> records = new List<TransformData>();
        private Transform target;

        [SerializeField] private float recordInterval = 1.0f; // 何秒ごとに記録
        [SerializeField] private float keepDuration = 5.0f;   // 最大保持時間（秒）
        [SerializeField] private int maxRecords = 100;        // 最大記録数

        private float timer = 0f;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
        private void Awake()
        {
            target = transform;
        }
        private void Update()
        {
            if (target == null) return;

            timer += Time.deltaTime;
            if (timer >= recordInterval)
            {
                RecordTransform();
                TrimOldRecords(); // 古い・多すぎる記録を削除
                timer = 0f;
            }
        }

        void RecordTransform()
        {
            records.Add(new TransformData(target));
        }

        void TrimOldRecords()
        {
            float cutoffTime = Time.time - keepDuration;

            // 古すぎる記録を削除（時間ベース）
            while (records.Count > 0 && records[0].timestamp < cutoffTime)
                records.RemoveAt(0);

            // 多すぎる記録を削除（件数ベース）
            while (records.Count > maxRecords)
                records.RemoveAt(0);
        }
        public void StartReplayFromSecondsAgo(float secondsAgo, Transform t)
        {
            if (records.Count == 0) return;
            StopAllCoroutines();
            StartCoroutine(ReplayCoroutine(secondsAgo, t));
        }

        private IEnumerator ReplayCoroutine(float secondsAgo, Transform t)
        {
            if (!t) yield break;

            float startTime = Time.time - secondsAgo;

            // ① startTime 以前の中で一番遅いデータ（＝直前の状態）を探す
            TransformData? first = null;
            for (int i = records.Count - 1; i >= 0; i--)
            {
                if (records[i].timestamp <= startTime)
                {
                    first = records[i];
                    break;
                }
            }

            // 安全処理：なければ先頭を使う（念のため）
            if (!first.HasValue)
                first = records[0];

            // 指定Transformに即時反映（補間せず代入）
            t.position = first.Value.position;
            t.rotation = first.Value.rotation;
            t.localScale = first.Value.scale;


            // ③ 再生対象データを取得（startTime以降）
            List<TransformData> replayData = records.FindAll(d => d.timestamp >= startTime);

            if (replayData.Count < 2)
                yield break;

            // ④ 通常通り補間再生
            for (int i = 0; i < replayData.Count - 1; i++)
            {
                TransformData from = replayData[i];
                TransformData to = replayData[i + 1];
                float duration = to.timestamp - from.timestamp;
                float timer = 0f;

                while (timer < duration)
                {
                    if (!t) yield break;

                    timer += Time.deltaTime;
                    float tLerp = Mathf.Clamp01(timer / duration);

                    Vector3 pos = Vector3.Lerp(from.position, to.position, tLerp);
                    Quaternion rot = Quaternion.Slerp(from.rotation, to.rotation, tLerp);
                    Vector3 scale = Vector3.Lerp(from.scale, to.scale, tLerp);

                    t.position = pos;
                    t.rotation = rot;
                    t.localScale = scale;

                    yield return null;
                }
            }
        }
    }
}