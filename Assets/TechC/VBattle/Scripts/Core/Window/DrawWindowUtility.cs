using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace TechC.VBattle.Core.Window
{
    /// <summary>
    /// ウィンドウに画像を描画するためのクラス
    /// </summary>
    public static class DrawWindowUtility
    {
        private const int BI_RGB = 0;
        private const uint DIB_RGB_COLORS = 0;

        /// <summary>
        /// ウィンドウにテクスチャを描画する。
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="texture">テクスチャ</param>
        /// <param name="drawWidth">横幅</param>
        /// <param name="drawHeight">高さ</param>
        /// <param name="orientation">向き</param>
        public static void DrawTextureToWindow(IntPtr hWnd, Texture2D texture, int drawWidth, int drawHeight, ImageOrientation orientation = ImageOrientation.Normal)
        {
            if (hWnd == IntPtr.Zero || texture == null) return;

            HWND hwnd = new HWND(hWnd);
            HDC hdc = PInvoke.GetDC(hwnd);
            if (hdc == HDC.Null) return;

            try
            {
                int srcWidth = texture.width;
                int srcHeight = texture.height;
                Color32[] pixels = texture.GetPixels32();
                byte[] bmpData = new byte[srcWidth * srcHeight * 4];

                for (int y = 0; y < srcHeight; y++)
                {
                    for (int x = 0; x < srcWidth; x++)
                    {
                        int srcX = x, srcY = y;

                        switch (orientation)
                        {
                            case ImageOrientation.FlipVertical:
                                srcY = srcHeight - 1 - y;
                                break;
                            case ImageOrientation.FlipHorizontal:
                                srcX = srcWidth - 1 - x;
                                break;
                            case ImageOrientation.Rotate180:
                                srcX = srcWidth - 1 - x;
                                srcY = srcHeight - 1 - y;
                                break;
                        }

                        // GDIは上下反転なのでさらに反転
                        int gdiY = srcHeight - 1 - srcY;
                        if (orientation == ImageOrientation.FlipVertical)
                        {
                            // 反転済みなのでそのまま
                            gdiY = srcY;
                        }
                        var pixel = pixels[srcY * srcWidth + srcX];
                        int idx = (gdiY * srcWidth + x) * 4;
                        bmpData[idx] = pixel.b;
                        bmpData[idx + 1] = pixel.g;
                        bmpData[idx + 2] = pixel.r;
                        bmpData[idx + 3] = pixel.a;
                    }
                }

                unsafe
                {
                    fixed (byte* pBmp = bmpData)
                    {
                        BITMAPINFO bmi = new BITMAPINFO();
                        bmi.bmiHeader.biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>();
                        bmi.bmiHeader.biWidth = srcWidth;
                        bmi.bmiHeader.biHeight = srcHeight;
                        bmi.bmiHeader.biPlanes = 1;
                        bmi.bmiHeader.biBitCount = 32;
                        bmi.bmiHeader.biCompression = BI_RGB;
                        bmi.bmiHeader.biSizeImage = (uint)(srcWidth * srcHeight * 4);

                        PInvoke.StretchDIBits(
                            hdc,
                            0, 0, drawWidth, drawHeight,
                            0, 0, srcWidth, srcHeight,
                            pBmp,
                            &bmi,
                            DIB_RGB_COLORS,
                            ROP_CODE.SRCCOPY
                        );
                    }
                }
            }
            finally
            {
                PInvoke.ReleaseDC(hwnd, hdc);
            }
        }
        public static void SetLayeredTexture(HWND hwnd, Texture2D tex)
        {
            int texWidth = tex.width;
            int texHeight = tex.height;

            var pixels = tex.GetPixels32();
            var buffer = new byte[texWidth * texHeight * 4];

            // Unity の RGBA32 → BGRA32
            for (int y = 0; y < texHeight; y++)
            {
                for (int x = 0; x < texWidth; x++)
                {
                    var px = pixels[y * texWidth + x];
                    int idx = (y * texWidth + x) * 4;
                    buffer[idx] = px.b;
                    buffer[idx + 1] = px.g;
                    buffer[idx + 2] = px.r;
                    buffer[idx + 3] = px.a;
                }
            }

            unsafe
            {
                fixed (byte* p = buffer)
                {
                    BITMAPINFO bmi = new BITMAPINFO();
                    bmi.bmiHeader.biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>();
                    bmi.bmiHeader.biWidth = texWidth;
                    bmi.bmiHeader.biHeight = texHeight;
                    bmi.bmiHeader.biPlanes = 1;
                    bmi.bmiHeader.biBitCount = 32;
                    bmi.bmiHeader.biCompression = BI_RGB;

                    HDC hdcScreen = PInvoke.GetDC(HWND.Null);

                    // ソースDC
                    HDC hdcSrc = PInvoke.CreateCompatibleDC(hdcScreen);
                    void* ppvBits;
                    HBITMAP hBitmapSrc = PInvoke.CreateDIBSection(
                        hdcScreen, &bmi, DIB_RGB_COLORS, &ppvBits, HANDLE.Null, 0);
                    Buffer.MemoryCopy(p, ppvBits, buffer.Length, buffer.Length);
                    HGDIOBJ oldSrc = PInvoke.SelectObject(hdcSrc, hBitmapSrc);

                    // ウィンドウサイズ取得
                    RECT rect;
                    PInvoke.GetClientRect(hwnd, out rect);
                    int wndWidth = rect.right - rect.left;
                    int wndHeight = rect.bottom - rect.top;
                    SIZE wndSize = new SIZE(wndWidth, wndHeight);

                    // 転送先DC（ウィンドウサイズにリサイズ）
                    HDC hdcDst = PInvoke.CreateCompatibleDC(hdcScreen);
                    HBITMAP hBitmapDst = PInvoke.CreateCompatibleBitmap(hdcScreen, wndWidth, wndHeight);
                    HGDIOBJ oldDst = PInvoke.SelectObject(hdcDst, hBitmapDst);

                    // ストレッチ転送（テクスチャサイズ → ウィンドウサイズ）
                    PInvoke.StretchBlt(
                        hdcDst,
                        0, 0, wndWidth, wndHeight,   // 転送先サイズ
                        hdcSrc,
                        0, 0, texWidth, texHeight,   // 転送元サイズ
                        ROP_CODE.SRCCOPY
                    );

                    // ブレンド設定
                    BLENDFUNCTION blend = new BLENDFUNCTION
                    {
                        BlendOp = (byte)PInvoke.AC_SRC_OVER,
                        BlendFlags = 0,
                        SourceConstantAlpha = 255,
                        AlphaFormat = (byte)PInvoke.AC_SRC_ALPHA
                    };

                    // リサイズ済みの hdcDst を使って更新
                    PInvoke.UpdateLayeredWindow(
                        hwnd,
                        hdcScreen,
                        new System.Drawing.Point(0, 0),
                        wndSize,
                        hdcDst,
                        new System.Drawing.Point(0, 0),
                        new COLORREF(0),
                        blend,
                        Windows.Win32.UI.WindowsAndMessaging.UPDATE_LAYERED_WINDOW_FLAGS.ULW_ALPHA
                    );

                    // 後始末
                    PInvoke.SelectObject(hdcSrc, oldSrc);
                    PInvoke.SelectObject(hdcDst, oldDst);
                    PInvoke.DeleteObject(hBitmapSrc);
                    PInvoke.DeleteObject(hBitmapDst);
                    PInvoke.DeleteDC(hdcSrc);
                    PInvoke.DeleteDC(hdcDst);
                    PInvoke.ReleaseDC(HWND.Null, hdcScreen);
                }
            }
        }

    }

    /// <summary>
    /// 画像の向きを表す列挙型
    /// </summary>
    public enum ImageOrientation
    {
        Normal,
        FlipVertical,
        FlipHorizontal,
        Rotate180,
    }
}