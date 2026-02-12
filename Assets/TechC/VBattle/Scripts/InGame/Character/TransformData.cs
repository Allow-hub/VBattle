using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 行動を再現するためのTransformの記録用構造体
    /// </summary>
    [System.Serializable]
    public struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public float timestamp;
        public TransformData(Transform t)
        {
            position = t.localPosition;
            rotation = t.localRotation;
            scale = t.localScale;
            timestamp = Time.time;
        }

        public void ApplyTo(Transform t)
        {
            t.localPosition = position;
            t.localRotation = rotation;
            t.localScale = scale;
        }
    }
}
