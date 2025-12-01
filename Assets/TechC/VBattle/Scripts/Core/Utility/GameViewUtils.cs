using UnityEngine;
using Windows.Win32.Foundation;

namespace TechC.VBattle.Core.Util
{
    /// <summary>
    /// ゲームビュー操作のUtil
    /// </summary>
    public static class GameViewUtils
    {
        /// <summary>
        /// ゲームビューのスクリーンRectを取得
        /// 現在はScreenから簡易的にみているためEditor上では正確ではない
        /// </summary>
        /// <returns></returns>
        public static Rect GetGameViewScreenRect()
        {
            var unityRect = new Rect(
               Screen.mainWindowPosition.x,
               Screen.mainWindowPosition.y,
               Screen.width,
               Screen.height
           );
            return unityRect;
        }

        /// <summary>
        /// RectをRECTに変換
        /// </summary>
        /// <param name="unityRect"></param>
        /// <returns></returns>
        public static RECT ToWin32Rect(Rect unityRect)
        {
            int left = Mathf.RoundToInt(unityRect.x);
            int top = Mathf.RoundToInt(unityRect.y);
            int right = Mathf.RoundToInt(unityRect.x + unityRect.width);
            int bottom = Mathf.RoundToInt(unityRect.y + unityRect.height);
            return new RECT
            {
                left = left,
                top = top,
                right = right,
                bottom = bottom
            };
        }
    }
}