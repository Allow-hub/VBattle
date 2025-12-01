using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.Systems
{
    /// <summary>
    /// オブジェクトプール
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [Header("Object Pool Settings")]
        [SerializeField] private List<ObjectPoolItem> poolItems = new List<ObjectPoolItem>(); // プールする項目のリスト
        [SerializeField] private bool autoExpand = true; // プールが空の場合に自動的に拡張するかどうか
        [SerializeField] private int expandSize = 5; // 自動拡張時のサイズ

        // 各プレハブごとのオブジェクトプール
        private Dictionary<GameObject, Queue<GameObject>> objectPools = new Dictionary<GameObject, Queue<GameObject>>();

        // インスタンスと元のプレハブを紐づける辞書
        private Dictionary<GameObject, ObjectPoolItem> instanceToPoolItemMap = new Dictionary<GameObject, ObjectPoolItem>();

        private void Awake()
        {
            InitializeAllPools();
        }

        /// <summary>
        /// すべてのプールを初期化します
        /// </summary>
        private void InitializeAllPools()
        {
            if (poolItems == null || poolItems.Count == 0)
            {
                Debug.LogWarning("Object Poolの初期化が不足しています。プールリストを設定してください。");
                return;
            }

            foreach (var poolItem in poolItems)
            {
                if (poolItem.prefab == null)
                {
                    Debug.LogError($"プール項目 '{poolItem.name}' のプレハブがnullです。");
                    continue;
                }

                if (poolItem.parent == null)
                {
                    // 親が指定されていない場合は、このオブジェクトを親として使用
                    poolItem.parent = this.gameObject;
                }

                InitializePool(poolItem);
            }
        }

        /// <summary>
        /// 指定したプレハブ用のプールを初期化します
        /// </summary>
        /// <param name="poolItem">初期化するプール項目</param>
        private void InitializePool(ObjectPoolItem poolItem)
        {
            if (poolItem.prefab == null)
            {
                Debug.LogError("プレハブがnullのプールを初期化できません。");
                return;
            }

            if (!objectPools.ContainsKey(poolItem.prefab))
            {
                objectPools[poolItem.prefab] = new Queue<GameObject>();
            }

            for (int i = 0; i < poolItem.initialSize; i++)
            {
                GameObject newObject = CreateNewInstance(poolItem);
                objectPools[poolItem.prefab].Enqueue(newObject);
            }
        }

        /// <summary>
        /// 新しいインスタンスを作成します
        /// </summary>
        /// <param name="poolItem">生成元となるプール項目</param>
        /// <returns>生成されたGameObject</returns>
        private GameObject CreateNewInstance(ObjectPoolItem poolItem)
        {
            if (poolItem.prefab == null)
            {
                Debug.LogError("nullのプレハブからインスタンスを作成できません。");
                return null;
            }

            GameObject newObject = Instantiate(poolItem.prefab);
            newObject.SetActive(false);

            if (poolItem.parent != null)
            {
                newObject.transform.SetParent(poolItem.parent.transform);
            }

            instanceToPoolItemMap[newObject] = poolItem;
            return newObject;
        }

        /// <summary>
        /// プールにプレハブを追加します
        /// </summary>
        /// <param name="name">プール項目の名前</param>
        /// <param name="prefab">プールするプレハブ</param>
        /// <param name="parent">親オブジェクト</param>
        /// <param name="initialSize">初期サイズ</param>
        public void AddToPool(string name, GameObject prefab, GameObject parent, int initialSize)
        {
            if (prefab == null)
            {
                Debug.LogError("nullのプレハブをプールに追加できません。");
                return;
            }

            // すでに同じプレハブがプールに存在するか確認
            if (objectPools.ContainsKey(prefab))
            {
                Debug.LogWarning($"プレハブ '{prefab.name}' はすでにプールに追加されています。");
                return;
            }

            ObjectPoolItem newItem = new ObjectPoolItem(name, prefab, parent ? parent : this.gameObject, initialSize);
            poolItems.Add(newItem);
            InitializePool(newItem);
        }

        /// <summary>
        /// プレハブからオブジェクトを取得します
        /// </summary>
        /// <param name="prefab">取得したいオブジェクトのプレハブ</param>
        /// <returns>アクティブ化されたGameObject</returns>
        public GameObject GetObject(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("nullのプレハブからオブジェクトを取得できません。");
                return null;
            }

            // プールが存在し、オブジェクトがある場合
            if (objectPools.TryGetValue(prefab, out Queue<GameObject> pool) && pool.Count > 0)
            {
                GameObject pooledObject = pool.Dequeue();

                // nullチェック（破棄されたオブジェクトの対応）
                if (pooledObject == null)
                {
                    // nullの場合は新しいインスタンスを作成
                    ObjectPoolItem poolItem = poolItems.Find(item => item.prefab == prefab);
                    if (poolItem != null)
                    {
                        pooledObject = CreateNewInstance(poolItem);
                    }
                }

                pooledObject.SetActive(true);
                return pooledObject;
            }
            else
            {
                // 初期リストに含まれるプレハブか確認
                ObjectPoolItem poolItem = poolItems.Find(item => item.prefab == prefab);
                if (poolItem != null)
                {
                    if (autoExpand)
                    {
                        // プールの自動拡張
                        ExpandPool(poolItem, expandSize);

                        // 拡張後に再度オブジェクトを取得
                        if (objectPools[prefab].Count > 0)
                        {
                            GameObject pooledObject = objectPools[prefab].Dequeue();
                            pooledObject.SetActive(true);
                            return pooledObject;
                        }
                    }

                    // 拡張しない場合または拡張後もプールが空の場合は新しいインスタンスを作成
                    GameObject newObject = CreateNewInstance(poolItem);
                    newObject.SetActive(true);
                    return newObject;
                }
                else
                {
                    Debug.LogWarning($"要求されたプレハブ '{prefab.name}' は ObjectPool に登録されていません。");
                    return null;
                }
            }
        }

        /// <summary>
        /// プレハブからオブジェクトを取得し、指定した位置と回転を設定します
        /// </summary>
        /// <param name="prefab">取得したいオブジェクトのプレハブ</param>
        /// <param name="position">設定する位置</param>
        /// <param name="rotation">設定する回転</param>
        /// <returns>アクティブ化されたGameObject</returns>
        public GameObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject obj = GetObject(prefab);
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
            }
            return obj;
        }

        /// <summary>
        /// 名前で指定したプールからオブジェクトを取得します
        /// </summary>
        /// <param name="poolName">プール名</param>
        /// <returns>アクティブ化されたGameObject</returns>
        public GameObject GetObjectByName(string poolName)
        {
            if (string.IsNullOrEmpty(poolName))
            {
                Debug.LogError("プール名がnullまたは空です。");
                return null;
            }

            ObjectPoolItem poolItem = poolItems.Find(item => item.name == poolName);
            if (poolItem != null && poolItem.prefab != null)
            {
                return GetObject(poolItem.prefab);
            }

            // Debug.LogWarning($"名前 '{poolName}' のプールが見つかりません。");
            return null;
        }

        /// <summary>
        /// プールを指定したサイズに拡張します
        /// </summary>
        /// <param name="poolItem">拡張するプール項目</param>
        /// <param name="expandCount">拡張するオブジェクト数</param>
        private void ExpandPool(ObjectPoolItem poolItem, int expandCount)
        {
            if (poolItem == null || poolItem.prefab == null)
            {
                Debug.LogError("無効なプール項目でプールを拡張できません。");
                return;
            }

            for (int i = 0; i < expandCount; i++)
            {
                GameObject newObject = CreateNewInstance(poolItem);
                objectPools[poolItem.prefab].Enqueue(newObject);
            }

            // Debug.Log($"プール '{poolItem.name}' を {expandCount} 個拡張しました。現在のサイズ: {objectPools[poolItem.prefab].Count}");
        }

        /// <summary>
        /// オブジェクトをプールに返却します
        /// </summary>
        /// <param name="obj">プールに返却するオブジェクト</param>
        public void ReturnObject(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("nullのオブジェクトをプールに返却できません。");
                return;
            }

            // オブジェクトをリセット（位置・回転・スケール）
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            obj.SetActive(false);

            if (instanceToPoolItemMap.TryGetValue(obj, out ObjectPoolItem poolItem))
            {
                if (poolItem.parent != null)
                {
                    obj.transform.SetParent(poolItem.parent.transform);
                }

                if (objectPools.ContainsKey(poolItem.prefab))
                {
                    objectPools[poolItem.prefab].Enqueue(obj);
                }
                else
                {
                    // プールが見つからない場合は新しく作成
                    objectPools[poolItem.prefab] = new Queue<GameObject>();
                    objectPools[poolItem.prefab].Enqueue(obj);
                }
            }
            else
            {
                Debug.LogWarning($"オブジェクト '{obj.name}' はプールに登録されていません。削除します。");
                Destroy(obj);
            }
        }

        /// <summary>
        /// すべてのプールの統計情報を取得します
        /// </summary>
        /// <returns>各プールのサイズを含む文字列</returns>
        public string GetPoolStats()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Object Pool 統計:");

            foreach (var poolItem in poolItems)
            {
                if (poolItem.prefab != null && objectPools.ContainsKey(poolItem.prefab))
                {
                    sb.AppendLine($"プール '{poolItem.name}': {objectPools[poolItem.prefab].Count} オブジェクト");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// すべてのプールを空にします
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in objectPools.Values)
            {
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
            }

            objectPools.Clear();
            instanceToPoolItemMap.Clear();

            // プールを再初期化
            InitializeAllPools();

            Debug.Log("すべてのオブジェクトプールをクリアしました。");
        }

        /// <summary>
        /// 自動でプールの配列内にアイテムをセットします
        /// </summary>
        /// <param name="item"></param>
        public void AddPoolItem(ObjectPoolItem item)
        {
            poolItems.Add(item);
        }
        
        /// <summary>
        /// 全てのプール内の非アクティブオブジェクトに任意の処理を実行します
        /// </summary>
        /// <param name="action">処理するアクション。GameObjectを引数に取ります</param>
        public void ForEachInactiveInPool(System.Action<GameObject> action)
        {
            if (action == null) return;

            foreach (var poolPair in objectPools)
            {
                foreach (var obj in poolPair.Value)
                {
                    if (obj != null && !obj.activeInHierarchy)
                    {
                        action(obj);
                    }
                }
            }
        }

    }
}