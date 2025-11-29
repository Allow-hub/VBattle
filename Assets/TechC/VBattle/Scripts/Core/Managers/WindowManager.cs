using UnityEngine;
using System.Collections.Generic;
using Windows.Win32.Foundation;
using Windows.Win32;
using TechC.VBattle.Core.Window;
using TechC.VBattle.Core.Util;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace TechC.VBattle.Core.Managers
{
    /// <summary>
    /// ウィンドウの管理クラス
    /// </summary>
    public class WindowManager : Singleton<WindowManager>
    {
        [SerializeField] private Sprite image;
        private List<NativeWindow> normalWindows = new();
        private List<NativeWindow> colliderWindows = new();

        private Dictionary<NativeWindow, GameObject> windowColliders = new();

        private bool allreadyPopup = false;
        public bool AllreadyPopup => allreadyPopup;
        [Header("コライダー移動許可範囲")]
        public Vector2 areaCenter = Vector2.zero;
        public Vector2 areaSize = new Vector2(10, 6);

        public override void Init()
        {
            base.Init();
            // DelayUtility.StartDelayedActionAsync(0.1f, () =>
            // {
            //     var w = WindowFactory.I.GetWindow(WindowFactory.WindowType.Image);
            //     WindowUtility.MoveWindow((HWND)w.Hwnd, 500, 500);
            //     if(w is ImageWindow imageWindow)
            //         imageWindow.SetImage(image.texture);
                
            // });
        }

        void Update()
        {
            foreach (var w in colliderWindows)
            {
                if (windowColliders.TryGetValue(w, out var colliderObj) && colliderObj != null)
                {
                    UpdateColliderTransform(w, colliderObj);
                }   
            }
        }


        private void UpdateColliderTransform(NativeWindow window, GameObject colliderObj)
        {
            window.SetRect();
            var unityRect = WindowUtility.GetUnityGameViewRect();//gameWindow
            RECT nativeRect;
            if (window is WebWindow webWindow)
                PInvoke.GetWindowRect(webWindow.WebWindowHwnd, out nativeRect);
            else
                PInvoke.GetWindowRect((HWND)window.Hwnd, out nativeRect);

            int centerX = (nativeRect.left + nativeRect.right) / 2;//オブジェクトの原点をウィンドウの中心に置くため
            int centerY = (nativeRect.top + nativeRect.bottom) / 2;
            int relativeX = centerX - unityRect.left;
            int relativeY = centerY - unityRect.top;
            int flippedY = unityRect.Height - relativeY;//UnityとWindowsの座標系はyが反転

            float targetZ = -5.3f;
            float camZ = Camera.main.transform.position.z;
            float depth = Mathf.Abs(targetZ - camZ);//カメラからターゲットの位置までの距離

            Vector3 screenPos = new Vector3(relativeX, flippedY, depth);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            worldPos.z = targetZ; // 安全のため上書き
            Vector3 clampPos = ClampToAllowedArea(worldPos);
            //オブジェクトの原点ではなくオブジェクトの前面の中心を使う
            Vector3 frontOffset = colliderObj.transform.forward * (colliderObj.transform.localScale.z * 0.5f);
            colliderObj.transform.position = clampPos - frontOffset;
            Vector3 worldSize = GetWindowSizeInWorldUnits(nativeRect.Width, nativeRect.Height, Camera.main);
            colliderObj.transform.localScale = worldSize;
        }


        private Vector3 ClampToAllowedArea(Vector3 worldPos)
        {
            float halfWidth = areaSize.x * 0.5f;
            float halfHeight = areaSize.y * 0.5f;

            float clampedX = Mathf.Clamp(worldPos.x, areaCenter.x - halfWidth, areaCenter.x + halfWidth);
            float clampedY = Mathf.Clamp(worldPos.y, areaCenter.y - halfHeight, areaCenter.y + halfHeight);

            return new Vector3(clampedX, clampedY, worldPos.z);
        }

        private Vector3 GetWindowSizeInWorldUnits(float pixelWidth, float pixelHeight, Camera camera)
        {
            if (camera == null)
            {
                Debug.LogError("Camera is null!");
                return Vector3.one;
            }

            float zDepth = -5.3f; // ウィンドウコライダーの Z 座標に合わせる

            // スクリーン座標系での中心 + 幅・高さ
            Vector3 screenCenter = camera.WorldToScreenPoint(new Vector3(0f, 0f, zDepth));
            Vector3 screenRight = screenCenter + new Vector3(pixelWidth, 0f, 0f);
            Vector3 screenTop = screenCenter + new Vector3(0f, pixelHeight, 0f);

            Vector3 worldCenter = camera.ScreenToWorldPoint(screenCenter);
            Vector3 worldRight = camera.ScreenToWorldPoint(screenRight);
            Vector3 worldTop = camera.ScreenToWorldPoint(screenTop);

            float worldWidth = Mathf.Abs(worldRight.x - worldCenter.x);
            float worldHeight = Mathf.Abs(worldTop.y - worldCenter.y);

            return new Vector3(worldWidth, worldHeight, 1f);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(areaCenter.x, areaCenter.y, -5.3f), new Vector3(areaSize.x, areaSize.y, 0.1f));
        }

        public void PopupWindowWindow(WindowFactory.WindowType type, int maxSize = 500, float duration = 1f, Sprite tex = null)
        {
            // 画面サイズ取得
            var unityRect = GameViewUtils.ToWin32Rect(GameViewUtils.GetGameViewScreenRect());
            int unityScreenX = unityRect.left;
            int unityScreenY = unityRect.top;
            int unityScreenWidth = unityRect.right - unityRect.left;
            int unityScreenHeight = unityRect.bottom - unityRect.top;
            int tileSize = unityScreenHeight / 6;


            // Windowで隙間なく覆うための分割数を計算
            var rnd = new System.Random();

            // 横・縦に何枚並べるか
            int xCount = Mathf.CeilToInt((float)unityScreenWidth / tileSize);
            int yCount = Mathf.CeilToInt((float)unityScreenHeight / tileSize);

            int windowCount = xCount * yCount;
            float interval = duration / windowCount;

            // グリッドの全パターンをリスト化してシャッフル
            List<(int xi, int yi)> gridList = new List<(int xi, int yi)>();
            for (int xi = 0; xi < xCount; xi++)
                for (int yi = 0; yi < yCount; yi++)
                    gridList.Add((xi, yi));

            // シャッフル
            for (int i = gridList.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                var tmp = gridList[i];
                gridList[i] = gridList[j];
                gridList[j] = tmp;
            }

            int created = 0;
            DelayUtility.StartRepeatedActionAsync(duration, interval, () =>
            {
                if (created >= gridList.Count)
                {
                    allreadyPopup = true;
                    return UniTask.CompletedTask;
                }

                var (xi, yi) = gridList[created];

                int remainWidth = unityScreenWidth - xi * tileSize;
                int remainHeight = unityScreenHeight - yi * tileSize;

                int wMin = Mathf.Min(tileSize, remainWidth);
                int wMax = Mathf.Min(maxSize, remainWidth);
                int hMin = Mathf.Min(tileSize, remainHeight);
                int hMax = Mathf.Min(maxSize, remainHeight);

                int w = (wMin < wMax) ? rnd.Next(wMin, wMax + 1) : wMin;
                int h = (hMin < hMax) ? rnd.Next(hMin, hMax + 1) : hMin;

                int x = unityScreenX + xi * tileSize;
                int y = unityScreenY + yi * tileSize;

                var win = WindowFactory.I.GetWindow(type);
                WindowUtility.MoveWindow((HWND)win.Hwnd, x, y);
                WindowUtility.ResizeWindow((HWND)win.Hwnd, w, h);
                win.SetRect();

                if (win is ImageWindow imageWindow)
                    imageWindow.SetImage(tex.texture, w, h);

                normalWindows.Add(win);
                created++;

                return UniTask.CompletedTask;
            });
        }
        public void ResetAllreasyPopup() => allreadyPopup = false;
        
        /// <summary>
        /// すべてのウィンドウとコライダーをリリース・破棄する
        /// </summary>
        public void ReleaseAllWindows()
        {
            // 通常ウィンドウをリリース
            foreach (var window in normalWindows)
            {
                WindowFactory.I.ReturnWindow(window);
            }
            normalWindows.Clear();

            // コライダーウィンドウをリリース
            foreach (var window in colliderWindows)
            {
                if (windowColliders.TryGetValue(window, out var obj))
                {
                    WindowColliderFactory.I.ReturnWindowCollider(obj);
                }
            }
            colliderWindows.Clear();
            windowColliders.Clear();

            // フラグもリセット
            allreadyPopup = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public Vector2 WorldToWindowsScreenPosition(Vector3 worldPosition, Camera camera = null)
        {
            if (camera == null) camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("Camera is null!");
                return Vector2.zero;
            }


            Vector3 unityScreenPos = camera.WorldToScreenPoint(worldPosition);

            var gameViewRect = WindowUtility.GetUnityGameViewRect(); // RECT

            // RECTの値を直接使用
            float winX = gameViewRect.left + unityScreenPos.x;
            float winY = gameViewRect.top + (gameViewRect.Height - unityScreenPos.y);

            return new Vector2(winX, winY);
        }

        private int GetLayerFromMask(int mask)
        {
            for (int i = 0; i < 32; i++)
            {
                if ((mask & (1 << i)) != 0)
                    return i;
            }
            Debug.LogWarning("No valid layer found in mask. Defaulting to 0.");
            return 0;
        }

        public void AddColliderWindow(NativeWindow nativeWindow, LayerMask? layerMask = null)
        {
            LayerMask actualLayerMask = layerMask ?? (1 << LayerMask.NameToLayer("WindowObj"));

            var windowCollider = WindowColliderFactory.I.GetWindowColliderPrefab();
            windowCollider.layer = GetLayerFromMask(actualLayerMask.value);
            windowColliders[nativeWindow] = windowCollider;

            colliderWindows.Add(nativeWindow);
        }

        public void RemoveColliderWindow(NativeWindow nativeWindow)
        {
            if (colliderWindows.Contains(nativeWindow))
            {
                colliderWindows.Remove(nativeWindow);

                if (windowColliders.TryGetValue(nativeWindow, out var obj))
                {
                    WindowColliderFactory.I.ReturnWindowCollider(obj);
                    windowColliders.Remove(nativeWindow);
                }
            }
        }

        /// <summary>
        /// アニメーション無しでウィンドウを非表示に
        /// </summary>
        /// <param name="returnAllWindow">WindowをすべてReturnするか</param>
        /// <param name="nativeWindow">WindowをすべてReturnしない場合何のウィンドウを非表示にするか</param>
        public void ResetWindow(bool returnAllWindow, NativeWindow nativeWindow = null)
        {
            if (returnAllWindow)
            {
                foreach (var window in normalWindows)
                    WindowFactory.I.ReturnWindow(window);
            }
            else
            {
                WindowFactory.I.ReturnWindow(nativeWindow);
            }
        }

        protected override void OnRelease()
        {
            normalWindows.Clear();
            colliderWindows.Clear();
            base.OnRelease();
        }
    }
}