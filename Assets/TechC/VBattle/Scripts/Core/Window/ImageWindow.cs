using System;
using UnityEngine;
using Windows.Win32.Foundation;

namespace TechC.VBattle.Core.Window
{
    /// <summary>
    /// 画像を表示するウィンドウクラス
    /// ImageLayered: 枠なし・レイヤードウィンドウ（透過可能）
    /// Image: 枠付き・通常ウィンドウ
    /// </summary>
    public class ImageWindow : NativeWindow
    {
        private Texture2D image;
        private int targetX = 0;
        private int targetY = 0;

        /// <summary>
        /// レイヤードウィンドウかどうか
        /// </summary>
        public bool IsLayered => Type == WindowFactory.WindowType.ImageLayered;

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
                
                if (IsLayered)
                {
                    // レイヤードウィンドウの場合は位置情報も含めて描画
                    DrawWindowUtility.SetLayeredTextureWithPosition((HWND)Hwnd, image, targetX, targetY);
                }
                else
                {
                    // 通常ウィンドウの場合はGDI描画
                    DrawWindowUtility.DrawTextureToWindow(Hwnd, image, Width, Height, ImageOrientation.FlipVertical);
                }
            }
        }

        /// <summary>
        /// ウィンドウの位置を設定
        /// レイヤードウィンドウ: UpdateLayeredWindowで位置も含めて更新
        /// 通常ウィンドウ: MoveWindowで位置を変更
        /// </summary>
        public void SetPosition(int x, int y)
        {
            targetX = x;
            targetY = y;
            
            if (IsLayered && image != null)
            {
                // レイヤードウィンドウは位置込みで再描画
                DrawWindowUtility.SetLayeredTextureWithPosition((HWND)Hwnd, image, targetX, targetY);
            }
            else
            {
                // 通常ウィンドウはMoveWindowを使用
                WindowUtility.MoveWindow((HWND)Hwnd, x, y);
            }
        }

        /// <summary>
        /// 画像を設定
        /// 引数を省略した場合このウィンドウのサイズを用いる
        /// </summary>
        public void SetImage(Texture2D texture, int? drawWidth = null, int? drawHeight = null, int widthMargin = 10, int heightMargin = 40)
        {
            image = texture;
            SetRect();
            
            if (IsLayered)
            {
                // レイヤードウィンドウの場合
                DrawWindowUtility.SetLayeredTextureWithPosition((HWND)Hwnd, image, targetX, targetY);
            }
            else
            {
                // 通常ウィンドウの場合はGDI描画
                int w = drawWidth ?? Width;
                int h = drawHeight ?? Height;
                w -= widthMargin; // ウィンドウの枠を考慮
                h -= heightMargin; // ウィンドウの枠を考慮
                DrawWindowUtility.DrawTextureToWindow(Hwnd, image, w, h, ImageOrientation.FlipVertical);
            }
        }

        /// <summary>
        /// テクスチャをレイヤードウィンドウとして描画
        /// レイヤードウィンドウの場合のみ使用可能
        /// </summary>
        public void SetTextureToBitmap(Texture2D texture)
        {
            //レイヤードが選択されてない可能性があるので修正の余地あり
            // if (!IsLayered)
            // {
            //     Debug.LogWarning("SetTextureToBitmap is only available for ImageLayered windows. Use SetImage instead.");
            //     return;
            // }
            
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