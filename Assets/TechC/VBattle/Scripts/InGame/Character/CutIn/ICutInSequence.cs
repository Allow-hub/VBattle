using System;
using Cysharp.Threading.Tasks;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// カットイン演出のインターフェース
    /// </summary>
    public interface ICutInSequence
    {
        /// <summary>
        /// 必殺技演出を開始
        /// </summary>
        UniTask Play();

        /// <summary>
        /// 演出が終了したときに呼ぶ
        /// </summary>
        event Action OnFinished;
    }
}
