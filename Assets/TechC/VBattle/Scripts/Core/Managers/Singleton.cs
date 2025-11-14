using UnityEngine;
 
namespace TechC.VBattle.Core.Managers
{
    /// <summary>
    /// シングルトンの基底クラス,シーンをまたがない場合は明示的な初期化が必要
    /// </summary>
    /// <typeparam name="T">クラス名</typeparam>
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        /// <summary>
        /// 派生クラスでこの値を変更して、DontDestroyOnLoad を使うかどうかを制御する
        /// </summary>
        protected virtual bool UseDontDestroyOnLoad => true;
 
        /// <summary>
        /// 重複時に GameObject ごと破壊するか（false だとこのコンポーネントだけ破壊）
        /// </summary>
        protected virtual bool DestroyTargetGameObject => false;
    
        public static T I { get; private set; } = null;
 
        public static bool IsValid() => I != null;
 
        private bool isInitialized = false;

        protected void Awake()
        {
            // 明示的な初期化が必要な場合はAwakeでの自動初期化をスキップ
            if (UseDontDestroyOnLoad)
                InitializeSingleton();
        }

        public void InitializeSingleton()
        {
            // 既に初期化済みの場合は重複として扱う
            if (isInitialized || (I != null && I != this))
            {
                if (DestroyTargetGameObject)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(this);
                }
                return;
            }

            I = this as T;
            isInitialized = true;
            I.Init();

            if (UseDontDestroyOnLoad)
            {
                DontDestroyOnLoad(this.gameObject);
            }
        }

        private void OnDestroy()
        {
            if (I == this)
            {
                OnRelease();
                // UseDontDestroyOnLoad = falseの場合は、必ずインスタンス参照をクリア
                if (!UseDontDestroyOnLoad)
                {
                    I = null;
                }
                // DontDestroyOnLoadの場合は、このインスタンスが本当に破棄される時のみクリア
                else if (this.gameObject.scene.name == "DontDestroyOnLoad")
                {
                    I = null;
                }
            }
        }

        /// <summary>
        /// 派生クラス用の初期化メソッド
        /// </summary>
        public virtual void Init()
        {
            I = this as T;
            isInitialized = true;
        }

        /// <summary>
        /// 派生クラス用の破棄処理
        /// </summary>
        protected virtual void OnRelease() { }
    }
}