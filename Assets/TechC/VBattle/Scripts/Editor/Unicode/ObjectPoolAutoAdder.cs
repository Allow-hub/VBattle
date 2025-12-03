using UnityEngine;
using UnityEditor;
using System.IO;
using TechC.VBattle.Systems;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Util;

namespace TechC.VBattle.Editor.Unicode
{
    /// <summary>
    /// ObjectPoolに指定フォルダ内のPrefabを一括追加するエディターツール
    /// 既存のObjectPoolに対して、フォルダ内のすべてのPrefabを自動で追加します
    /// </summary>
    public class ObjectPoolAutoAdder : EditorWindow
    {
        private ObjectPool targetPool;
        private GameObject parentObject;
        private string prefabFolderPath = "Assets/TechC/VBattle/Prefabs/InGame/Comments/Unicode";

        [MenuItem("Tools/Unicode/ObjectPool Auto Adder")]
        public static void ShowWindow()
        {
            GetWindow<ObjectPoolAutoAdder>("ObjectPool Auto Adder");
        }

        void OnGUI()
        {
            GUILayout.Label("Add Prefabs from Folder to ObjectPool", EditorStyles.boldLabel);

            targetPool = (ObjectPool)EditorGUILayout.ObjectField("Target ObjectPool", targetPool, typeof(ObjectPool), true);

            parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true);

            prefabFolderPath = EditorGUILayout.TextField("Prefab Folder Path", prefabFolderPath);

            if (GUILayout.Button("Add all Prefabs from folder"))
            {
                if (targetPool == null)
                {
                    CustomLogger.Error("Target ObjectPool is not set.", LogTagUtil.TagEditor);
                    return;
                }

                if (parentObject == null)
                {
                    CustomLogger.Warning("Parent Object is not set. Using targetPool GameObject as parent.", LogTagUtil.TagEditor);
                    parentObject = targetPool.gameObject;
                }

                AddPrefabsToPool();
            }
        }

        void AddPrefabsToPool()
        {
            string fullPath = Application.dataPath.Replace("Assets", "") + prefabFolderPath;

            if (!Directory.Exists(fullPath))
            {
                CustomLogger.Error($"Folder not found: {prefabFolderPath}", LogTagUtil.TagEditor);
                return;
            }

            string[] prefabFiles = Directory.GetFiles(fullPath, "*.prefab", SearchOption.TopDirectoryOnly);

            if (prefabFiles.Length == 0)
            {
                CustomLogger.Warning("No prefab files found in folder.", LogTagUtil.TagEditor);
                return;
            }

            int addedCount = 0;

            foreach (string filePath in prefabFiles)
            {
                string assetPath = "Assets" + filePath.Replace(Application.dataPath, "").Replace('\\', '/');
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                if (prefab == null)
                {
                    CustomLogger.Warning($"Could not load prefab at {assetPath}", LogTagUtil.TagEditor);
                    continue;
                }

                ObjectPoolItem newItem = new ObjectPoolItem(prefab.name, prefab, parentObject, 5);

                targetPool.AddPoolItem(newItem);

                addedCount++;
            }

            EditorUtility.SetDirty(targetPool);
            AssetDatabase.SaveAssets();

            CustomLogger.Info($"{addedCount} prefab(s) added to the ObjectPool.", LogTagUtil.TagEditor);
        }
    }
}
