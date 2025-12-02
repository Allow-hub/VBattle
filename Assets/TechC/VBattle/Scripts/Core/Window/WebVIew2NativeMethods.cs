using System.IO.Pipes;
using System.IO;

namespace TechC.VBattle.Core.Window
{
    /// <summary>
    /// WebView2のDLLインポートメソッドを定義するクラス。
    /// </summary>
    internal static class WebView2NativeMethods
    {
        /// <summary>
        /// URLまたはHTMLを送信してWebView2を更新するメソッド。
        /// このメソッドは、WebView2のネイティブアプリケーションと通信するためにNamedPipeを使用します。
        /// </summary>
        /// <param name="content"></param>
        public static void SendContentToWebView2(string content)
        {
            using (var pipe = new NamedPipeClientStream(".", "WebView2Pipe", PipeDirection.Out))
            {
                pipe.Connect(1000);
                using (var writer = new StreamWriter(pipe))
                {
                    writer.WriteLine(content);
                    writer.Flush();
                }
            }
        }
    }
}
