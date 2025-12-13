using UnityEngine;

namespace TechC.VBattle.InGame.Camera
{
    /// <summary>
    /// カメラエフェクトの共通インターフェース
    /// </summary>
    public interface ICameraEffect
    {
        /// <summary>
        /// エフェクトを初期化する
        /// </summary>
        /// <param name="cameraTransform">対象のカメラTransform</param>
        void Initialize(Transform cameraTransform);

        /// <summary>
        /// エフェクトの現在の状態
        /// </summary>
        CameraEffectState State { get; }

        /// <summary>
        /// エフェクトを適用する
        /// </summary>
        /// <param name="intensity">エフェクトの強度</param>
        /// <param name="duration">エフェクトの継続時間</param>
        void Apply();

        /// <summary>
        /// エフェクトを停止する
        /// </summary>
        void Stop();

        /// <summary>
        /// エフェクトをリセットして初期状態に戻す
        /// </summary>
        void Reset();
    }
}
