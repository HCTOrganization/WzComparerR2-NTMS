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

        public static string customCheckUri { get; set; } = "https://www.example.com/";
        public static string customCheckCondition { get; set; } = "https://www.example.com/directoryCondition";
        public string currentUri { get; private set; } = "";

        private async void WebViewDialog_Load(object sender, EventArgs e)
        {
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.WebResourceResponseReceived += WebResourceResponseReceived;
            webView.Source = new Uri(webViewUri);
        }

        private async void WebResourceResponseReceived(object sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            if (webView.Source.ToString().Contains(customCheckUri))
            {
                if (webView.Source.ToString().Contains(customCheckCondition))
                {
                    try
                    {
                        currentUri = webView.CoreWebView2.Source;

                        this.Invoke((Action)(() => this.Close()));
                    }
                    catch
                    {
                    }
                }
                else
                {
                    ScrollToElement("anc02");
                }
            }
            else
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

        private async void ScrollToElement(string elementId)
        {
            try
            {
                string script = $"document.getElementById('{elementId}').scrollIntoView();";
                await webView.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                throw ex;
            }
}
    }

    public class MyJsonModel
    {
    }
}
#endif