using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TechC.VBattle.Core;
using TechC.VBattle.Core.Managers;
using UnityEngine;
// using Windows.Win32.Foundation;

namespace TechC.Select
{
    /// <summary>
    /// セレクト画面で2キャラ選択したときにStart？をウィンドウを使ったデザインをだす
    /// </summary>
    public class StartWindow : Singleton<StartWindow>
    {
        // [SerializeField, ReadOnly] private string appearChars = "Start?";
        // private string[] chars;//appearCharsが一文字づつ入る
        // private int windowCount;
        // private bool isAnimating = false;
        // private List<NativeWindow> windows = new List<NativeWindow>();

        // #region  それぞれの文字のWindow設定
        // [SerializeField] private List<Vector2> windowTop;//ウィンドウの位置（原点は左上）
        // [SerializeField] private List<int> windowHeight;
        // [SerializeField] private List<int> windowWidth;
        // [SerializeField] private List<Sprite> windowImage;
        // private List<(NativeWindow,string, Vector2, int, int, Sprite)> windowSettings=new List<(NativeWindow, string, Vector2, int, int, Sprite)>();
        // #endregion

        // protected override bool UseDontDestroyOnLoad => false;

        // private void Start()
        // {
        //     var count = appearChars.Length;
        //     windowCount = count;
        //     chars = new string[count];
        //     chars = appearChars.Select(c => c.ToString()).ToArray();
        //     for (var i = 0; i < windowCount; i++)
        //     {
        //         var window = WindowFactory.I.GetWindow(WindowFactory.WindowType.Image);
        //         windows.Add(window);
        //         WindowUtility.ResizeWindow((HWND)window.Hwnd, 0, 0);
        //         WindowUtility.MoveWindow((HWND)window.Hwnd, -1920, -1080);//初期値を画面外に
        //         var setting = (window, chars[i], windowTop[i], windowHeight[i], windowWidth[i], windowImage[i]);
        //         windowSettings.Add(setting);
        //     }
        //     // ShowStartWindow();
        // }

        // private void Update()
        // {
        //     if (!isAnimating) return;
        //     foreach (var (w, ch, top, height, width, sp) in windowSettings)
        //     {
        //         var imageWindow = (ImageWindow)w;
        //         imageWindow.SetImage(sp.texture);
        //         WindowUtility.MoveWindow((HWND)w.Hwnd, (int)top.x, (int)top.y);
        //         WindowUtility.AnimateResizeWindow(w.Hwnd, width, height);
        //     }
        // }

        // public void ShowStartWindow()
        // {
        //     isAnimating = true;
        // }

        // public void HideStartWindow()
        // {

        // }
    }
}
