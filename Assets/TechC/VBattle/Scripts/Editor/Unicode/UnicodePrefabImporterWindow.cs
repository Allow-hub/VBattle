using UnityEditor;
using UnityEngine;
using System.IO;
using TechC.VBattle.InGame.Comment;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Util;

namespace TechC.VBattle.Editor.Unicode
{
    /// <summary>
    /// CharPrefabDatabaseにUnicodePrefabを自動登録するエディターツール
    /// 指定フォルダ内のUnicodePrefabを解析し、CharPrefabDatabaseに文字とPrefabの対応関係を自動登録します
    /// </summary>
    public class UnicodePrefabImporterWindow : EditorWindow
    {
        private string prefabFolderPath = "Assets/TechC/VBattle/Prefabs/InGame/Comments/Unicode";
        private CharPrefabDatabase targetDatabase;

        [MenuItem("Tools/Unicode/Unicode Prefab Importer")]
        public static void ShowWindow()
        {
            GetWindow<UnicodePrefabImporterWindow>("Unicode Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Unicode プレハブ自動登録ツール", EditorStyles.boldLabel);

            // パス入力欄
            EditorGUILayout.LabelField("Unicode プレハブフォルダ");
            prefabFolderPath = EditorGUILayout.TextField(prefabFolderPath);

            // ScriptableObject アセットを選択
            targetDatabase = (CharPrefabDatabase)EditorGUILayout.ObjectField(
                "登録先のCharPrefabDatabase.asset",
                targetDatabase,
                typeof(CharPrefabDatabase),
                false
            );

            EditorGUILayout.Space();

            // 黄緑色の追加ボタン
            GUI.backgroundColor = new Color(0.6f, 1.0f, 0.6f);  // 黄緑色
            if (GUILayout.Button("フォルダ内のPrefabから追加"))
            {
                if (targetDatabase == null)
                {
                    CustomLogger.Warning("ScriptableObject が選択されていません。", LogTagUtil.TagEditor);
                    return;
                }
                AddPrefabsFromFolder();
            }

            if (GUILayout.Button("テスト追加（あ〜こ）"))
            {
                if (targetDatabase == null)
                {
                    CustomLogger.Warning("ScriptableObject が選択されていません。", LogTagUtil.TagEditor);
                    return;
                }
                AddTestCharacters();
            }

            GUI.backgroundColor = Color.white; // 色リセット
            EditorGUILayout.Space();

            // 赤色のリセットボタン
            if (targetDatabase != null)
            {
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("CharPrefabDatabase.entriesをリセット（空にする）"))
                {
                    if (EditorUtility.DisplayDialog(
                        "確認",
                        "本当にCharPrefabDatabase.entriesの内容をすべて削除しますか？",
                        "はい",
                        "キャンセル"))
                    {
                        ResetEntries();
                    }
                }
                GUI.backgroundColor = Color.white; // 色リセット
            }
        }


        private void AddPrefabsFromFolder()
        {
            string[] prefabPaths = Directory.GetFiles(prefabFolderPath, "U*.prefab", SearchOption.TopDirectoryOnly);

            int count = 0;

            foreach (string path in prefabPaths)
            {
                string fileName = Path.GetFileNameWithoutExtension(path); // 例: U3042

                if (fileName.StartsWith("U") &&
                    int.TryParse(fileName.Substring(1), System.Globalization.NumberStyles.HexNumber, null, out int codePoint))
                {
                    string character = char.ConvertFromUtf32(codePoint);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    if (prefab != null && !targetDatabase.entries.Exists(e => e.charText == character))
                    {
                        targetDatabase.entries.Add(new CharPrefabEntry
                        {
                            charText = character,
                            charPrefab = prefab
                        });
                        count++;
                    }
                }
            }

            EditorUtility.SetDirty(targetDatabase);
            AssetDatabase.SaveAssets();

            CustomLogger.Info($"{count}個のプレハブが追加されました！", LogTagUtil.TagUnicode);
        }

        private void AddTestCharacters()
        {
            string testChars = "あいうえおかきくけこ";
            int count = 0;

            foreach (char c in testChars)
            {
                string hex = ((int)c).ToString("X4");
                string prefabName = $"U{hex}.prefab";
                string fullPath = Path.Combine(prefabFolderPath, prefabName);

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
                if (prefab != null && !targetDatabase.entries.Exists(e => e.charText == c.ToString()))
                {
                    targetDatabase.entries.Add(new CharPrefabEntry
                    {
                        charText = c.ToString(),
                        charPrefab = prefab
                    });
                    count++;
                }
            }

            EditorUtility.SetDirty(targetDatabase);
            AssetDatabase.SaveAssets();

            CustomLogger.Info($"テストで {count} 個追加されました！", LogTagUtil.TagUnicode);
        }

        private void ResetEntries()
        {
            Undo.RecordObject(targetDatabase, "Reset entries");
            targetDatabase.entries.Clear();
            EditorUtility.SetDirty(targetDatabase);
            AssetDatabase.SaveAssets();
            CustomLogger.Info("CharPrefabDatabase.entries を空にリセットしました。", LogTagUtil.TagUnicode);
        }
    }
}
