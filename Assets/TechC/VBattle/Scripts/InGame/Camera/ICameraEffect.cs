using UnityEngine;

namespace TechC.VBattle.InGame.Camera
{
    /// <summary>
    /// カメラエフェクトの共通インターフェース
    /// </summary>
    public interface ICameraEffect
    {
        CameraEffectState State { get; }

        void Init(Transform cameraTransform);
        void Apply();
        void Stop(Vector3 originalPosition);
        void Reset(Vector3 originalPosition);
    }
}