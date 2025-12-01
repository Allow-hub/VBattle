using System.IO;
using UnityEngine;

namespace TechC.VBattle.Core.Window
{
    public static class HtmlNames
    {
        public enum HtmlFileName
        {
            //必要に応じて定義してください
            Test,
            Error,//Ame必殺用
            Wall,
            // About
        }
        /// <summary>
        /// Enumに対応したファイル名を取得
        /// </summary>
        private static string GetFileName(HtmlFileName htmlFile)
        {
            return htmlFile switch
            {
                HtmlFileName.Test => "Test.html",
                HtmlFileName.Error => "Error.html",
                HtmlFileName.Wall => "Wall.html",
                // HtmlFileName.About => "About.html",
                _ => null
            };
        }

        /// <summary>
        /// StreamingAssets/Htmls 内のHTMLのパスを返す
        /// </summary>
        public static string LoadHtmlFromStreamingAssets(HtmlFileName htmlFile)
        {
            string fileName = GetFileName(htmlFile);
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError($"HtmlNames: ファイル名が不明です: {htmlFile}");
                return $"<html><body><h2>不明なHTMLファイルです: {htmlFile}</h2></body></html>";
            }

            string path = Path.Combine(Application.streamingAssetsPath, "Htmls", fileName);

            if (!File.Exists(path))
            {
                Debug.LogError($"HtmlNames: ファイルが見つかりません: {path}");
                return $"<html><body><h2>ファイルが見つかりません: {fileName}</h2></body></html>";
            }

            return path;
        }
    }
}