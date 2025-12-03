using UnityEngine;
using UnityEditor;
using System.IO;

namespace TechC.VBattle.Editor.Unicode
{
    public class FbxToPrefabConverter : EditorWindow
    {
        private string fbxFolderPath = "Assets/TechC/VBattle/Models/3DText/Unicode"; // FBXが入っているフォルダ
        private string prefabOutputPath = "Assets/TechC/VBattle/Prefabs/Comments/Unicode"; // 出力先Prefabフォルダ

        [MenuItem("Tools/Unicode/FBX to Prefab Converter")]
        public static void ShowWindow()
        {
            GetWindow<FbxToPrefabConverter>("FBX to Prefab Converter");
        }

        private void OnGUI()
        {
            GUILayout.Label("FBX to Prefab Converter", EditorStyles.boldLabel);

            fbxFolderPath = EditorGUILayout.TextField("FBX Folder Path", fbxFolderPath);
            prefabOutputPath = EditorGUILayout.TextField("Prefab Output Path", prefabOutputPath);

            if (GUILayout.Button("Convert FBX to Prefabs"))
            {
                ConvertFbxToPrefabs(fbxFolderPath, prefabOutputPath);
            }
        }

        private void ConvertFbxToPrefabs(string fbxPath, string prefabPath)
        {
            if (!AssetDatabase.IsValidFolder(fbxPath))
            {
                Debug.LogError($"無効なFBXフォルダ: {fbxPath}");
                return;
            }

            if (!AssetDatabase.IsValidFolder(prefabPath))
            {
                Directory.CreateDirectory(prefabPath);
                AssetDatabase.Refresh();
            }

            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { fbxPath });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                if (fbx == null) continue;

                string prefabName = Path.GetFileNameWithoutExtension(assetPath);
                string outputPath = Path.Combine(prefabPath, prefabName + ".prefab");

                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(fbx, outputPath);
                Debug.Log($"Prefabを作成: {outputPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
