using System;
using UnityEngine;
using Windows.Win32.Foundation;

namespace TechC.VBattle.Core.Window
{
    /// <summary>
    /// 画像を表示するウィンドウクラス
    /// </summary>
    public class ImageWindow : NativeWindow
    {
        private Texture2D image;

        public ImageWindow(IntPtr hwnd, int width, int height, Texture2D texture)
            : base(hwnd, width, height, WindowFactory.WindowType.Image)
        {
            image = texture;
        }

        public override void Show()
        {
            base.Show();
            if (image != null)
            {
                SetRect();
                DrawWindowUtility.DrawTextureToWindow(Hwnd, image, Width, Height);
            }
        }


        /// <summary>
        /// 引数を省略した場合このウィンドウのサイズを用いる
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="drawWidth"></param>
        /// <param name="drawHeight"></param>
        public void SetImage(Texture2D texture, int? drawWidth = null, int? drawHeight = null, int widthMargin = 10, int heightMargin = 40)
        {
            image = texture;
            SetRect();
            int w = drawWidth ?? Width;
            int h = drawHeight ?? Height;
            w -= widthMargin; // ウィンドウの枠を考慮
            h -= heightMargin; // ウィンドウの枠を考慮
            DrawWindowUtility.DrawTextureToWindow(Hwnd, image, w, h, ImageOrientation.FlipVertical);
        }
        /// <summary>
        /// テクスチャをリドローしない
        /// </summary>
        /// <param name="texture"></param>
        public void SetTextureToBitmap(Texture2D texture)
        {
            image = texture;
            SetRect();
            DrawWindowUtility.SetLayeredTexture((HWND)Hwnd, image);
        }
        public override void Destroy()
        {
            base.Destroy();
        }
    }
}
