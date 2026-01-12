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
        private int targetX = 0;
        private int targetY = 0;

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
                // レイヤードウィンドウの場合は位置情報も含めて描画
                DrawWindowUtility.SetLayeredTextureWithPosition((HWND)Hwnd, image, targetX, targetY);
            }
        }

        /// <summary>
        /// レイヤードウィンドウの位置を設定
        /// </summary>
        public void SetPosition(int x, int y)
        {
            targetX = x;
            targetY = y;
            
            if (image != null)
            {
                DrawWindowUtility.SetLayeredTextureWithPosition((HWND)Hwnd, image, targetX, targetY);
            }
        }

        /// <summary>
        /// 引数を省略した場合このウィンドウのサイズを用いる
        /// </summary>
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
        /// テクスチャをレイヤードウィンドウとして描画
        /// </summary>
        public void SetTextureToBitmap(Texture2D texture)
        {
            image = texture;
            SetRect();
            DrawWindowUtility.SetLayeredTextureWithPosition((HWND)Hwnd, image, targetX, targetY);
        }
        
        public override void Destroy()
        {
            base.Destroy();
        }
    }
}