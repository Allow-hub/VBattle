namespace TechC.VBattle.Editors
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// フォルダーにメモを残せる機能
    /// </summary>
    [InitializeOnLoad]
    public class FolderMemoEditor : Editor
    {
        private const string MEMO_FILE_NAME = ".folder_memo";
        private static Dictionary<string, string> memoCache = new Dictionary<string, string>();

        static FolderMemoEditor()
        {
            Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        }
        
        private static void OnPostHeaderGUI(Editor editor)
        {
            // 選択されたオブジェクトがフォルダかどうかをチェック
            if (editor.targets.Length == 1 && editor.target is DefaultAsset)
            {
                string assetPath = AssetDatabase.GetAssetPath(editor.target);

                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    DrawFolderMemo(assetPath);
                }
            }
        }

        private static void DrawFolderMemo(string folderPath)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("フォルダメモ", EditorStyles.boldLabel);

            // メモの読み込み
            string memo = LoadMemo(folderPath);

            // メモの表示と編集
            EditorGUI.BeginChangeCheck();
            string newMemo = EditorGUILayout.TextArea(memo, GUILayout.MinHeight(60));

            if (EditorGUI.EndChangeCheck())
            {
                SaveMemo(folderPath, newMemo);
            }

            EditorGUILayout.Space(5);
        }

        private static string LoadMemo(string folderPath)
        {
            // キャッシュをチェック
            if (memoCache.ContainsKey(folderPath))
            {
                return memoCache[folderPath];
            }

            // メモファイルのパスを取得
            string memoFilePath = Path.Combine(folderPath, MEMO_FILE_NAME);

            if (File.Exists(memoFilePath))
            {
                try
                {
                    string memo = File.ReadAllText(memoFilePath);
                    memoCache[folderPath] = memo;
                    return memo;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"メモの読み込みに失敗しました: {e.Message}");
                }
            }

            memoCache[folderPath] = string.Empty;
            return string.Empty;
        }

        private static void SaveMemo(string folderPath, string memo)
        {
            // キャッシュを更新
            memoCache[folderPath] = memo;

            string memoFilePath = Path.Combine(folderPath, MEMO_FILE_NAME);

            try
            {
                if (string.IsNullOrEmpty(memo))
                {
                    // メモが空の場合はファイルを削除
                    if (File.Exists(memoFilePath))
                    {
                        File.Delete(memoFilePath);
                        string metaFilePath = memoFilePath + ".meta";
                        if (File.Exists(metaFilePath))
                        {
                            File.Delete(metaFilePath);
                        }
                        AssetDatabase.Refresh();
                    }
                }
                else
                {
                    // メモを保存
                    File.WriteAllText(memoFilePath, memo);
                    AssetDatabase.Refresh();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"メモの保存に失敗しました: {e.Message}");
            }
        }
    }
}