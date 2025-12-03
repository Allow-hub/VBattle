using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Util;

namespace TechC.VBattle.Editor.Unicode
{
    /// <summary>
    /// 文字列をUnicode形式に変換し、CSファイルに出力するエディターツール
    /// 入力された文字をUnicodeコードポイントに変換し、指定されたCSファイルに書き込みます
    /// </summary>
    public class UnicodeConverterTool : EditorWindow
    {
        private string inputText = "";
        private MonoScript targetScript = null;
        private Vector2 scrollPosition;

        [MenuItem("Tools/Unicode/Unicode Converter")]
        public static void ShowWindow()
        {
            GetWindow<UnicodeConverterTool>("Unicode Converter");
        }

        private void OnGUI()
        {
            GUILayout.Label("Unicode Converter Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // テキスト入力エリア
            GUILayout.Label("入力テキスト:");
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));
            inputText = EditorGUILayout.TextArea(inputText, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);

            // ファイル選択エリア
            GUILayout.Label("出力先CSファイル:");
            targetScript = (MonoScript)EditorGUILayout.ObjectField(
                "Target CS File",
                targetScript,
                typeof(MonoScript),
                false
            );

            GUILayout.Space(10);

            // 変換ボタン
            GUI.enabled = !string.IsNullOrEmpty(inputText) && targetScript != null;

            if (GUILayout.Button("Unicodeに変換して出力", GUILayout.Height(30)))
            {
                ConvertAndOutput();
            }

            GUI.enabled = true;

            GUILayout.Space(10);

            // 使用方法の説明
            EditorGUILayout.HelpBox(
                "使用方法:\n" +
                "1. 上のテキストエリアに変換したい文字を入力\n" +
                "2. 出力先のCSファイルをドラッグ&ドロップで選択\n" +
                "3. 「Unicodeに変換して出力」ボタンをクリック\n\n" +
                "例: 「あ」→ (\"あ\", \"U3042\")",
                MessageType.Info
            );
        }

        private void ConvertAndOutput()
        {
            try
            {
                if (string.IsNullOrEmpty(inputText))
                {
                    CustomLogger.Error("入力テキストが空です。", LogTagUtil.TagUnicode);
                    return;
                }

                if (targetScript == null)
                {
                    CustomLogger.Error("出力先のCSファイルが選択されていません。", LogTagUtil.TagUnicode);
                    return;
                }

                // スクリプトファイルのパスを取得
                string scriptPath = AssetDatabase.GetAssetPath(targetScript);

                if (string.IsNullOrEmpty(scriptPath) || !scriptPath.EndsWith(".cs"))
                {
                    CustomLogger.Error("選択されたファイルが有効なCSファイルではありません。", LogTagUtil.TagUnicode);
                    return;
                }

                // Unicodeに変換
                StringBuilder result = new StringBuilder();
                int charCount = 0;

                foreach (char c in inputText)
                {
                    if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')
                    {
                        continue; // 制御文字はスキップ（改行、復帰、タブは除く）
                    }

                    if (charCount > 0)
                    {
                        result.Append(", ");
                    }

                    // 5文字ごとに改行
                    if (charCount > 0 && charCount % 5 == 0)
                    {
                        result.Append("\n");
                    }

                    // Unicode値を取得（16進数、4桁でゼロパディング）
                    string unicodeValue = "U" + ((int)c).ToString("X4");
                    result.Append($"(\"{c}\", \"{unicodeValue}\")");
                    charCount++;
                }

                if (charCount == 0)
                {
                    CustomLogger.Warning("変換可能な文字が見つかりませんでした。", LogTagUtil.TagUnicode);
                    return;
                }

                // ファイルに書き込み
                string fullPath = Path.GetFullPath(scriptPath);

                // 既存のファイル内容を読み取り
                string existingContent = "";
                if (File.Exists(fullPath))
                {
                    existingContent = File.ReadAllText(fullPath, Encoding.UTF8);
                }

                // 新しい内容を追加
                string newContent = existingContent + "\n" + result.ToString() + "\n";

                // ファイルに書き込み
                File.WriteAllText(fullPath, newContent, Encoding.UTF8);

                // Unityにファイルの変更を通知
                AssetDatabase.ImportAsset(scriptPath);
                AssetDatabase.Refresh();

                CustomLogger.Info($"Unicode変換完了: {charCount}文字を変換し、{scriptPath}に出力しました。", LogTagUtil.TagUnicode);

                // 入力テキストをクリア
                inputText = "";
            }
            catch (System.Exception ex)
            {
                CustomLogger.Error($"Unicode変換中にエラーが発生しました: {ex.Message}", LogTagUtil.TagUnicode);
            }
        }

        private void OnInspectorUpdate()
        {
            // GUIの更新を促す
            Repaint();
        }
    }
}