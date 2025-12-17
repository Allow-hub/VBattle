using UnityEngine;

namespace TechC.VBattle.InGame.Camera
{
    /// <summary>
    /// カメラエフェクトの共通インターフェース
    /// </summary>
    public interface ICameraEffect
    {
        /// <summary>
        /// エフェクトの現在の状態
        /// </summary>
        CameraEffectState State { get; }

        /// <summary>
        /// エフェクトを初期化する
        /// </summary>
        /// <param name="cameraTransform">対象のカメラTransform</param>
        void Initialize(Transform cameraTransform);

        /// <summary>
        /// エフェクトを適用する
        /// </summary>
        void Apply();

        /// <summary>
        /// エフェクトを停止する
        /// </summary>
        /// <param name="originalPosition">カメラの元の位置</param>
        void Stop(Vector3 originalPosition);

        /// <summary>
        /// エフェクトをリセットして初期状態に戻す
        /// </summary>
        /// <param name="originalPosition">カメラの元の位置</param>
        void Reset(Vector3 originalPosition);
    }
}