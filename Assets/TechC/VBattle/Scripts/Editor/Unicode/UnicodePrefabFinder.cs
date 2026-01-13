using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Util;

namespace TechC.VBattle.Editor.Unicode
{
    /// <summary>
    /// 3D文字ツール用のウィンドウクラス（プレースホルダー）
    /// キーボードショートカット用のヘルプメッセージを表示します
    /// </summary>
    public class My3DCharToolWindow : EditorWindow
    {
        private const string Shortcut = "Ctrl + Shift + P";

        void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.HelpBox($"このウィンドウを開くショートカット: {Shortcut}", MessageType.Info);
            // ここにツールの内容を追加
        }
    }

    /// <summary>
    /// 文字列からUnicodePrefabを検索・配置・管理するエディターツール
    /// 入力された文字列に対応するUnicodePrefabを検索し、シーン内に配置・削除する機能を提供します
    /// </summary>
    public class UnicodePrefabFinder : EditorWindow
    {
        private string inputText = "";
        private List<GameObject> foundPrefabs = new List<GameObject>();
        private Vector2 scrollPosition = Vector2.zero;

        private static readonly string prefabFolderPath = "Assets/TechC/VBattle/Prefabs/Comments/Unicode";

        [MenuItem("Tools/Unicode/Unicode Prefab Finder")]
        public static void ShowWindow()
        {
            GetWindow<UnicodePrefabFinder>("Unicode Finder");
        }

        void OnGUI()
        {
            GUILayout.Label("文字列を入力してください", EditorStyles.boldLabel);
            inputText = EditorGUILayout.TextField("入力:", inputText);

            if (GUILayout.Button("プレハブ検索"))
            {
                FindUnicodePrefabs(inputText);
            }

            if (GUILayout.Button("横に並べてみる", GUILayout.Height(25)))
            {
                PlacePrefabsHorizontally();
            }

            // ここにクリーンボタンを追加
            if (GUILayout.Button("TestCharPrefab内をクリーンにする", GUILayout.Height(25)))
            {
                CleanTestCharPrefabChildren();
            }

            GUILayout.Space(10);

            if (foundPrefabs.Count > 0)
            {
                GUILayout.Label("検索結果（プレハブ一覧）:", EditorStyles.boldLabel);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                foreach (var prefab in foundPrefabs)
                {
                    EditorGUILayout.ObjectField(prefab.name, prefab, typeof(GameObject), false);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        void FindUnicodePrefabs(string input)
        {
            foundPrefabs.Clear();

            foreach (char c in input)
            {
                int codePoint = char.ConvertToUtf32(c.ToString(), 0);
                string unicodeName = "U" + codePoint.ToString("X4");

                string[] guids = AssetDatabase.FindAssets(unicodeName + " t:prefab", new[] { prefabFolderPath });

                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                    {
                        foundPrefabs.Add(prefab);
                    }
                }
            }
        }


        void PlacePrefabsHorizontally()
        {
            string parentName = "TestCharPrefab";

            // Scene上に親オブジェクトを検索
            GameObject parent = GameObject.Find(parentName);

            if (parent == null)
            {
                CustomLogger.Warning($"Scene上に親オブジェクト「{parentName}」が存在しません。先に作成してください。", LogTagUtil.TagUnicode);
                return; // 処理中断
            }

            float spacing = 2.0f; // プレハブ間の距離
            Vector3 startPosition = Vector3.zero;

            for (int i = 0; i < foundPrefabs.Count; i++)
            {
                GameObject prefab = foundPrefabs[i];
                Vector3 position = startPosition + new Vector3(i * spacing, 0, 0);

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if (instance != null)
                {
                    Undo.RegisterCreatedObjectUndo(instance, "Place Unicode Prefab");
                    instance.transform.position = position;
                    instance.transform.parent = parent.transform;  // ここで親をセット
                }
            }
        }


        void CleanTestCharPrefabChildren()
        {
            GameObject parentObject = GameObject.Find("TestCharPrefab");
            if (parentObject == null)
            {
                CustomLogger.Warning("親オブジェクト 'TestCharPrefab' がSceneに見つかりません。", LogTagUtil.TagUnicode);
                return;
            }

            int childCount = parentObject.transform.childCount;
            if (childCount == 0)
            {
                CustomLogger.Info("TestCharPrefabには子オブジェクトがありません。", LogTagUtil.TagUnicode);
                return;
            }

            // Undo登録用に子を一時リストに取得
            List<GameObject> children = new List<GameObject>();
            for (int i = 0; i < childCount; i++)
            {
                children.Add(parentObject.transform.GetChild(i).gameObject);
            }

            // 子オブジェクトを削除（Undo可能）
            foreach (var child in children)
            {
                Undo.DestroyObjectImmediate(child);
            }

            CustomLogger.Info($"TestCharPrefabの子オブジェクト{childCount}件を削除しました。", LogTagUtil.TagUnicode);
        }
    }
}