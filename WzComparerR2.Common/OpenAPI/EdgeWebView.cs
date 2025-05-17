#if NET6_0_OR_GREATER
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;

namespace WzComparerR2.OpenAPI
{
    public partial class EdgeWebView : DevComponents.DotNetBar.Office2007Form
    {
        public EdgeWebView()
        {
            InitializeComponent();
        }

        public string jsonResult { get; private set; }

        public static string webViewUri { get; set; }

        private async void WebViewDialog_Load(object sender, EventArgs e)
        {
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.WebResourceResponseReceived += WebResourceResponseReceived;
            webView.Source = new Uri(webViewUri);
        }

        private async void WebResourceResponseReceived(object sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            try
            {
                using Stream stream = await e.Response.GetContentAsync();
                using StreamReader reader = new StreamReader(stream);
                jsonResult = await reader.ReadToEndAsync();

                var jsonData = JsonSerializer.Deserialize<MyJsonModel>(jsonResult);
                // Close dialog after processing
                this.Invoke((Action)(() => this.Close()));
            }
            catch
            {
            }
        }
    }

    public class MyJsonModel
    {
    }
}
#endif