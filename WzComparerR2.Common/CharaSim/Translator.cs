using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CsvHelper;
using CsvHelper.Configuration;
using DevComponents.DotNetBar;
using Newtonsoft.Json.Linq;
using WzComparerR2.Config;

namespace WzComparerR2.CharaSim
{
    /// <summary>
    /// Provides translation, localization, and currency conversion services.
    /// Supports multiple translation engines including Google Translate, Naver Papago, Mozhi, and OpenAI-compatible APIs.
    /// </summary>
    public class Translator
    {
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

        #region Static Mappings

        // Language to Currency mapping
        private static readonly Dictionary<string, string> LanguageToCurrency = new Dictionary<string, string>()
        {
            { "ja", "jpy" },
            { "ko", "krw" },
            { "zh-CN", "cny" },
            { "en", "usd" },
            { "zh-TW", "twd" }
        };

        private static readonly Dictionary<string, string> CurrencyToLanguage = new Dictionary<string, string>()
        {
            { "jpy", "ja" },
            { "krw", "ko" },
            { "cny", "zh-CN" },
            { "usd", "en" },
            { "twd", "zh-TW" },
            { "sgd", "en" }
        };

        // Language code to localized language name
        private static readonly Dictionary<string, string> LanguageNames = new Dictionary<string, string>()
        {
            { "ja", "Japanese" },
            { "ko", "Korean" },
            { "zh-CN", "Simplified Chinese" },
            { "en", "English" },
            { "zh-TW", "Traditional Chinese" },
            { "yue", "Cantonese" }
        };

        // Currency code to localized currency symbol
        private static readonly Dictionary<string, string> CurrencySymbols = new Dictionary<string, string>()
        {
            { "jpy", "日圓" },
            { "krw", "韓幣" },
            { "cny", "人民幣" },
            { "usd", "美金" },
            { "twd", "台幣" },
            { "hkd", "港幣" },
            { "mop", "澳門幣" },
            { "sgd", "新幣" },
            { "eur", "歐元" },
            { "cad", "加幣" },
            { "aud", "澳洲幣" },
            { "myr", "馬來林吉特" },
        };

        // Translation engine names indexed by engine ID
        private static readonly Dictionary<int, string> EngineNames = new Dictionary<int, string>()
        {
            { 0, "google" },
            { 1, "mozhi-google" },
            { 2, "mozhi-deepl" },
            { 3, "mozhi-duckduckgobing" },
            { 4, "mozhi-mymemory" },
            { 5, "mozhi-yandex" },
            { 6, "naver" },
            { 9, "openai" }
        };

        // Mozhi engines that require special configuration
        private static readonly HashSet<int> MozhiEngines = new HashSet<int> { 1, 2, 3, 4, 5 };

        #endregion

        #region API Endpoints

        private const string GTranslateBaseURL = "https://translate.googleapis.com/translate_a/t";
        private const string NTranslateBaseURL = "https://naveropenapi.apigw.ntruss.com";
        private const string GlossaryTablePath = "TranslationCache/Glossary.csv";

        private static readonly List<string> CurrencyBaseURLs = new List<string>()
        {
            "https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies/",
            "https://latest.currency-api.pages.dev/v1/currencies/",
            "https://registry.npmmirror.com/@fawazahmed0/currency-api/latest/files/v1/currencies/"
        };

        public static string OAITranslateBaseURL { get; set; }

        #endregion

        #region Helper Methods - Path Management

        /// <summary>
        /// Gets the translation cache directory path for the specified engine.
        /// </summary>
        private static string GetEngineCachePath(int engineId, string languageCode = null)
        {
            string engineName = GetEngineName(engineId);
            string fileName = engineId == 9 && !string.IsNullOrEmpty(languageCode)
                ? $"{DefaultLanguageModel}_{languageCode}.json"
                : $"{languageCode}.json";
            return Path.Combine(GetTranslationCacheBasePath(), engineName, fileName);
        }

        /// <summary>
        /// Gets the engine name for the given engine ID.
        /// </summary>
        private static string GetEngineName(int engineId)
        {
            return EngineNames.TryGetValue(engineId, out var name) ? name : EngineNames[0];
        }

        /// <summary>
        /// Gets the base translation cache directory path.
        /// </summary>
        private static string GetTranslationCacheBasePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TranslationCache");
        }

        /// <summary>
        /// Gets the full glossary table path.
        /// </summary>
        private static string GetGlossaryTableFullPath()
        {
            return Path.Combine(GetTranslationCacheBasePath(), GlossaryTablePath);
        }

        #endregion

        #region Helper Methods - Configuration

        /// <summary>
        /// Gets the language name for the given language code.
        /// </summary>
        private static string GetLanguageName(string languageCode)
        {
            return LanguageNames.TryGetValue(languageCode, out var name) ? name : "Unknown";
        }

        /// <summary>
        /// Gets the currency symbol for the given currency code.
        /// </summary>
        private static string GetCurrencySymbol(string currencyCode)
        {
            return CurrencySymbols.TryGetValue(currencyCode, out var symbol) ? symbol : currencyCode;
        }

        /// <summary>
        /// Determines if the engine is a Mozhi-based engine.
        /// </summary>
        private static bool IsMozhiEngine(int engineId)
        {
            return MozhiEngines.Contains(engineId);
        }

        /// <summary>
        /// Gets the Mozhi engine code for the given engine ID.
        /// </summary>
        private static string GetMozhiEngineCode(int engineId)
        {
            return engineId switch
            {
                1 => "google",
                2 => "deepl",
                3 => "duckduckgo",
                4 => "mymemory",
                5 => "yandex",
                _ => "google"
            };
        }

        #endregion

        #region Helper Methods - Error Handling

        /// <summary>
        /// Logs translation errors for debugging purposes.
        /// </summary>
        private static void LogTranslationError(string engine, string text, Exception ex)
        {
            // TODO: Implement proper logging using NetworkLogger or similar
            // For now, this is a placeholder for future logging implementation
        }

        #endregion

        #region Translation Methods

        /// <summary>
        /// Translates text using Google Translate API.
        /// </summary>
        private static string GTranslate(string text, string desiredLanguage)
        {
            try
            {
                var postData = new StringContent("q=" + Uri.EscapeDataString(text), Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = _httpClient.PostAsync(GTranslateBaseURL + "?client=gtx&format=text&sl=auto&tl=" + desiredLanguage, postData).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                var result = JArray.Parse(responseString);
                return result[0][0].ToString();
            }
            catch (Exception ex)
            {
                LogTranslationError("GTranslate", text, ex);
                return text;
            }
        }

        /// <summary>
        /// Detects the language of the given text using Google Translate API.
        /// </summary>
        private static string GTranslateDetect(string text, string desiredLanguage)
        {
            try
            {
                var postData = new StringContent("q=" + Uri.EscapeDataString(text), Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = _httpClient.PostAsync(GTranslateBaseURL + "?client=gtx&format=text&sl=auto&tl=" + desiredLanguage, postData).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                var result = JArray.Parse(responseString);
                return result[0][1].ToString();
            }
            catch (Exception ex)
            {
                LogTranslationError("GTranslateDetect", text, ex);
                return "ja";
            }
        }

        /// <summary>
        /// Translates text using OpenAI-compatible API.
        /// </summary>
        private static string OAITranslate(string text, string desiredLanguage, bool singleLine = false)
        {
            bool sysMsgDefined = !string.IsNullOrEmpty(DefaultOpenAISystemMessage);
            if (!sysMsgDefined)
                DefaultOpenAISystemMessage = string.Format("You are an automated translator, and your response should only have translated result in {0} while keeping formatting indicators untouched. If the source language is already in {0}, just response the original text.", GetLanguageName(desiredLanguage));
            
            if (string.IsNullOrEmpty(OAITranslateBaseURL))
                OAITranslateBaseURL = "https://api.openai.com/v1";

            var postData = new JObject(
                new JProperty("model", DefaultLanguageModel),
                new JProperty("messages", new JArray(
                    new JObject(
                        new JProperty("role", "system"),
                        new JProperty("content", DefaultOpenAISystemMessage)
                    ),
                    new JObject(
                        new JProperty("role", "user"),
                        new JProperty("content", sysMsgDefined ? $"Please translate following in-game content into {GetLanguageName(desiredLanguage)}: {text}" : text)
                    )
                )),
                new JProperty("stream", false)
            );

            if (IsExtraParamEnabled)
            {
                postData.Add(new JProperty("temperature", DefaultLMTemperature));
                postData.Add(new JProperty("max_tokens", DefaultMaximumToken));
            }

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, OAITranslateBaseURL + "/chat/completions");
                requestMessage.Content = new StringContent(postData.ToString(), Encoding.UTF8, "application/json");
                
                if (!string.IsNullOrEmpty(DefaultTranslateAPIKey))
                {
                    var reqHeaders = JObject.Parse(DefaultTranslateAPIKey);
                    foreach (var property in reqHeaders.Properties())
                        requestMessage.Headers.TryAddWithoutValidation(property.Name, property.Value.ToString());
                }

                var response = _httpClient.SendAsync(requestMessage).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                var jrResponse = JObject.Parse(responseString);
                string responseResult = jrResponse.SelectToken("choices[0].message.content").ToString();

                // Clean up response text
                responseResult = CleanOpenAIResponse(responseResult);

                if (singleLine)
                {
                    return responseResult.Replace("\r\n", " ").Replace("\n", " ").Replace("  ", " ").Replace("\"", "").Trim();
                }

                return responseResult;
            }
            catch (Exception ex)
            {
                LogTranslationError("OAITranslate", text, ex);
                return text;
            }
        }

        /// <summary>
        /// Cleans up OpenAI response by removing artifacts and formatting.
        /// </summary>
        private static string CleanOpenAIResponse(string response)
        {
            if (response.Contains("</think>"))
            {
                response = response.Split(new[] { "</think>\n\n" }, StringSplitOptions.None)[1];
            }

            if (response.Contains("**\""))
            {
                response = response.Split(new[] { "**" }, StringSplitOptions.None)[1];
            }

            if (response.Contains("：\n\n") || response.Contains(":\n\n") || response.Contains(": \n\n"))
            {
                response = response.Split(new[] { "\n\n" }, StringSplitOptions.None)[1];
                if (response.Contains("\r"))
                    response = response.Split(new[] { "\r" }, StringSplitOptions.None)[0];
                if (response.Contains("\n"))
                    response = response.Split(new[] { "\n" }, StringSplitOptions.None)[0];
            }

            return response;
        }

        /// <summary>
        /// Translates text using Mozhi backend.
        /// </summary>
        private static JObject MTranslate(string text, string engine, string sourceLanguage, string desiredLanguage)
        {
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get,
                    $"{DefaultMozhiBackend}/api/translate?engine={engine}&from={sourceLanguage}&to={desiredLanguage}&text={Uri.EscapeDataString(text)}");
                requestMessage.Headers.Accept.ParseAdd("application/json");
                var response = _httpClient.SendAsync(requestMessage).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                return JObject.Parse(responseString);
            }
            catch (Exception ex)
            {
                LogTranslationError("MTranslate", text, ex);
                return JObject.Parse($"{{\"translated-text\": \"{text}\"}}");
            }
        }

        /// <summary>
        /// Translates text using Naver Papago API.
        /// </summary>
        private static JObject NTranslate(string text, string desiredLanguage)
        {
            try
            {
                var postContent = new StringContent(
                    $"source=auto&target={desiredLanguage}&text={Uri.EscapeDataString(text)}",
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded");
                
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, NTranslateBaseURL + "/nmt/v1");
                requestMessage.Headers.Accept.ParseAdd("application/json");
                requestMessage.Headers.TryAddWithoutValidation("X-NCP-APIGW-API-KEY-ID", GetKeyValue("X-NCP-APIGW-API-KEY-ID"));
                requestMessage.Headers.TryAddWithoutValidation("X-NCP-APIGW-API-KEY", GetKeyValue("X-NCP-APIGW-API-KEY"));
                requestMessage.Content = postContent;
                
                var response = _httpClient.SendAsync(requestMessage).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                return JObject.Parse(responseString);
            }
            catch (Exception ex)
            {
                LogTranslationError("NTranslate", text, ex);
                return JObject.Parse("{\"message\": {\"result\": {\"translatedText\": \"無效的Naver API金鑰\"}}}");
            }
        }

        /// <summary>
        /// Gets a value from the JSON API key configuration.
        /// </summary>
        private static string GetKeyValue(string jsonDictKey)
        {
            try
            {
                return JObject.Parse(DefaultTranslateAPIKey).SelectToken(jsonDictKey).ToString();
            }
            catch
            {
                return "";
            }
        }

        #endregion

        #region Text Utilities

        /// <summary>
        /// Checks if the string contains Korean characters.
        /// </summary>
        public static bool IsKoreanStringPresent(string checkString)
        {
            if (string.IsNullOrEmpty(checkString))
                return false;
            return checkString.Any(c => c >= '\uAC00' && c <= '\uD7A3');
        }

        /// <summary>
        /// Converts Hiragana to Katakana and normalizes to full-width form.
        /// </summary>
        public static string FullWidthKatakana(string inputString, bool ConvertHiragana = true)
        {
            if (string.IsNullOrEmpty(inputString))
                return inputString;

            if (ConvertHiragana)
            {
                var sb = new StringBuilder();
                foreach (char c in inputString)
                {
                    if (c >= 'ぁ' && c <= 'ゖ')
                        sb.Append((char)(c + 0x60));
                    else
                        sb.Append(c);
                }
                return sb.ToString().Normalize(NormalizationForm.FormKC);
            }
            else
            {
                return inputString.Normalize(NormalizationForm.FormKC);
            }
        }

        /// <summary>
        /// Converts hash-based color tags to HTML tags for translation.
        /// </summary>
        public static string ConvHashTagToHTMLTag(string orgText)
        {
            if (string.IsNullOrEmpty(orgText))
                return orgText;

            return orgText
                .Replace("#c", "<CHL>")
                .Replace("#", "</CHL>")
                .Replace("\\r\\n", "<BR/>")
                .Replace("\\n", "<BR/>");
        }

        /// <summary>
        /// Converts HTML tags back to hash-based color tags after translation.
        /// </summary>
        public static string ConvHTMLTagToHashTag(string orgText)
        {
            if (string.IsNullOrEmpty(orgText))
                return orgText;

            return Regex.Replace(
                Regex.Replace(
                    Regex.Replace(
                        Regex.Replace(
                            orgText.Replace("< ", "<").Replace(" >", ">"),
                            "<CHL>", "#c", RegexOptions.IgnoreCase),
                        "</CHL>", "#", RegexOptions.IgnoreCase),
                    "<BR/>", "\r\n", RegexOptions.IgnoreCase),
                "CHL>", "#c", RegexOptions.IgnoreCase);
        }

        #endregion

        #region Main Translation API

        /// <summary>
        /// Translates the given text using the configured translation engine.
        /// </summary>
        public static string TranslateString(string orgText, bool titleCase = false)
        {
            if (string.IsNullOrEmpty(orgText) || orgText == "(null)")
                return orgText;

            // Check cache first
            string translatedText = TryFetchCachedTranslationResult(orgText);
            if (!string.IsNullOrEmpty(translatedText))
                return ApplyTitleCase(translatedText, titleCase);

            // Preprocess with glossary
            string glossaryText = GlossaryPreProcess(orgText);
            string targetLanguage = DefaultDesiredLanguage;

            // Translate using configured engine
            translatedText = TranslateWithEngine(ConvHashTagToHTMLTag(glossaryText), targetLanguage);

            // Post-process with glossary
            translatedText = GlossaryPostProcess(translatedText, targetLanguage);
            translatedText = ConvHTMLTagToHashTag(translatedText);
            translatedText = ApplyTitleCase(translatedText, titleCase);

            // Cache and return
            CacheTranslationResult(orgText, translatedText);
            return translatedText;
        }

        /// <summary>
        /// Applies title case formatting to English text if requested.
        /// </summary>
        private static string ApplyTitleCase(string text, bool titleCase)
        {
            if (titleCase && DefaultDesiredLanguage == "en")
            {
                var cultureInfo = Thread.CurrentThread.CurrentCulture;
                var textInfo = cultureInfo.TextInfo;
                return textInfo.ToTitleCase(text);
            }
            return text;
        }

        /// <summary>
        /// Translates text using the configured engine with appropriate language mappings.
        /// </summary>
        private static string TranslateWithEngine(string text, string targetLanguage)
        {
            return DefaultPreferredTranslateEngine switch
            {
                0 => GTranslate(text, targetLanguage).Replace("＃", "#"),
                1 => MTranslateWithEngine(text, "google", targetLanguage),
                2 => DeepLTranslateWithEngine(text, targetLanguage),
                3 => DuckDuckGoTranslateWithEngine(text, targetLanguage),
                4 => MyMemoryTranslateWithEngine(text, targetLanguage),
                5 => YandexTranslateWithEngine(text, targetLanguage),
                6 => NTranslate(text, NormalizeLanguageForNaver(targetLanguage)).SelectToken("message.result.translatedText").ToString(),
                9 => OAITranslate(text, targetLanguage),
                _ => GTranslate(text, targetLanguage).Replace("＃", "#")
            };
        }

        /// <summary>
        /// Translates using Mozhi with Google engine.
        /// </summary>
        private static string MTranslateWithEngine(string text, string engine, string targetLanguage)
        {
            return MTranslate(text, engine, "auto", targetLanguage).SelectToken("translated-text").ToString().Replace("＃", "#");
        }

        /// <summary>
        /// Translates using Mozhi with DeepL engine.
        /// </summary>
        private static string DeepLTranslateWithEngine(string text, string targetLanguage)
        {
            var normalizedLang = targetLanguage.Contains("zh") || targetLanguage == "yue" ? "zh" : targetLanguage;
            return MTranslate(text, "deepl", "en", normalizedLang).SelectToken("translated-text").ToString().Replace("＃", "#");
        }

        /// <summary>
        /// Translates using Mozhi with DuckDuckGo engine.
        /// </summary>
        private static string DuckDuckGoTranslateWithEngine(string text, string targetLanguage)
        {
            var normalizedLang = targetLanguage == "zh-CN" ? "zh" : targetLanguage;
            return MTranslate(text, "duckduckgo", "auto", normalizedLang).SelectToken("translated-text").ToString().Replace("＃", "#");
        }

        /// <summary>
        /// Translates using Mozhi with MyMemory engine.
        /// </summary>
        private static string MyMemoryTranslateWithEngine(string text, string targetLanguage)
        {
            var normalizedLang = targetLanguage.Contains("zh") || targetLanguage == "yue" ? "zh" : targetLanguage;
            return MTranslate(text, "mymemory", "Autodetect", normalizedLang).SelectToken("translated-text").ToString().Replace("＃", "#");
        }

        /// <summary>
        /// Translates using Mozhi with Yandex engine.
        /// </summary>
        private static string YandexTranslateWithEngine(string text, string targetLanguage)
        {
            var normalizedLang = targetLanguage.Contains("zh") || targetLanguage == "yue" ? "zh" : targetLanguage;
            return MTranslate(text, "yandex", "auto", normalizedLang).SelectToken("translated-text").ToString().Replace("＃", "#");
        }

        /// <summary>
        /// Normalizes language code for Naver API compatibility.
        /// </summary>
        private static string NormalizeLanguageForNaver(string lang)
        {
            return lang == "yue" ? "zh-TW" : lang;
        }

        /// <summary>
        /// Detects the language of the given text.
        /// </summary>
        public static string GetLanguage(string orgText)
        {
            if (string.IsNullOrEmpty(orgText) || orgText == "(null)")
                return "ja";

            return DefaultPreferredTranslateEngine switch
            {
                0 => GTranslateDetect(orgText, DefaultDesiredLanguage),
                1 => MTranslate(orgText, "google", "auto", DefaultDesiredLanguage).SelectToken("detected").ToString(),
                2 => DetectWithDeepL(orgText),
                3 => DetectWithDuckDuckGo(orgText),
                4 => DetectWithMyMemory(orgText),
                5 => DetectWithYandex(orgText),
                6 => NTranslate(orgText, NormalizeLanguageForNaver(DefaultDesiredLanguage)).SelectToken("message.result.srcLangType").ToString(),
                _ => "ja"
            };
        }

        private static string DetectWithDeepL(string text) => 
            MTranslate(text, "deepl", "en", "zh").SelectToken("detected").ToString();

        private static string DetectWithDuckDuckGo(string text) => 
            MTranslate(text, "duckduckgo", "auto", "zh").SelectToken("detected").ToString();

        private static string DetectWithMyMemory(string text) => 
            MTranslate(text, "mymemory", "Autodetect", "zh").SelectToken("detected").ToString();

        private static string DetectWithYandex(string text) => 
            MTranslate(text, "yandex", "auto", "zh").SelectToken("detected").ToString();

        #endregion

        #region String Merging

        /// <summary>
        /// Merges two strings based on layout preference.
        /// </summary>
        public static string MergeString(string text1, string text2, int newLineCounts = 0, bool oneLineSeparatorRequired = false, bool bracketRequiredForText2 = false)
        {
            if (text1 == text2)
                return text1;

            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return text1;

            return DefaultPreferredLayout switch
            {
                1 => MergeStringLayout1(text1, text2, newLineCounts, oneLineSeparatorRequired, bracketRequiredForText2),
                2 => MergeStringLayout2(text1, text2, newLineCounts, oneLineSeparatorRequired, bracketRequiredForText2),
                3 => text2,
                _ => text1
            };
        }

        private static string MergeStringLayout1(string text1, string text2, int newLineCounts, bool oneLineSeparatorRequired, bool bracketRequiredForText2)
        {
            var sb = new StringBuilder(text2);
            
            if (newLineCounts == 0 && oneLineSeparatorRequired)
                sb.Append(" / ");
            if (newLineCounts == 0 && bracketRequiredForText2)
                sb.Append(" ");

            for (int i = 0; i < newLineCounts; i++)
                sb.Append(Environment.NewLine);

            if (bracketRequiredForText2)
                sb.Append("(");
            
            sb.Append(text1);
            
            if (bracketRequiredForText2)
                sb.Append(")");

            return sb.ToString();
        }

        private static string MergeStringLayout2(string text1, string text2, int newLineCounts, bool oneLineSeparatorRequired, bool bracketRequiredForText2)
        {
            var sb = new StringBuilder(text1);
            
            if (newLineCounts == 0 && oneLineSeparatorRequired)
                sb.Append(" / ");
            if (newLineCounts == 0 && bracketRequiredForText2)
                sb.Append(" ");

            for (int i = 0; i < newLineCounts; i++)
                sb.Append(Environment.NewLine);

            if (bracketRequiredForText2)
                sb.Append("(");
            
            sb.Append(text2);
            
            if (bracketRequiredForText2)
                sb.Append(")");

            return sb.ToString();
        }

        #endregion

        #region CSV/Index Lookup

        /// <summary>
        /// Tries to get a translated string from CSV index for the given object ID.
        /// </summary>
        public static bool TryCheckStringIndex(int objectID, string indexType, out string translateResult)
        {
            string targetLanguage = DefaultDesiredLanguage;
            string typeIndex = Path.Combine(GetTranslationCacheBasePath(), $"{indexType}_{targetLanguage}.csv");
            
            if (File.Exists(typeIndex))
            {
                try
                {
                    var csvLookup = new CsvLookup(typeIndex);
                    translateResult = csvLookup.GetNameById(objectID);
                    return !string.IsNullOrEmpty(translateResult);
                }
                catch
                {
                    translateResult = "";
                    return false;
                }
            }

            translateResult = "";
            return false;
        }

        #endregion

        #region Tooltip Processing

        /// <summary>
        /// Processes and translates AFRM tooltip text before copying.
        /// </summary>
        public static string AfrmTooltipTranslateBeforeCopy(string orgText)
        {
            var preTranslateDict = ConvAfrmTooltipPreTextToDict(orgText);
            var postTranslateContent = new StringBuilder();
            var untranslatedContent = new StringBuilder();

            // Process cached translations
            foreach (string tag in preTranslateDict.Keys.ToList())
            {
                string translatedContent = TryFetchCachedTranslationResult(preTranslateDict[tag]);
                if (!string.IsNullOrEmpty(translatedContent))
                {
                    preTranslateDict.Remove(tag);
                    postTranslateContent.AppendLine($"<{tag}>{translatedContent}</{tag}>");
                }
            }

            // Translate remaining content
            if (preTranslateDict.Count > 1)
            {
                foreach (string tag in preTranslateDict.Keys.ToList())
                {
                    untranslatedContent.AppendLine($"<{tag}>{preTranslateDict[tag]}</{tag}>");
                }
                postTranslateContent.AppendLine(TranslateString(untranslatedContent.ToString()));
            }
            else if (preTranslateDict.Count == 1)
            {
                string tag = preTranslateDict.Keys.ToList()[0];
                postTranslateContent.AppendLine($"<{tag}>{TranslateString(preTranslateDict[tag])}</{tag}>");
            }

            // Reformat output
            var postTranslateDict = ConvAfrmTooltipPreTextToDict(postTranslateContent.ToString());
            postTranslateContent.Clear();

            foreach (string tag in new[] { "name", "desc", "pdesc", "autodesc", "hdesc", "descleftalign" })
            {
                if (!orgText.Contains($"<{tag}>"))
                    continue;
                postTranslateContent.AppendLine($"<{tag}>{postTranslateDict[tag]}</{tag}>");
            }

            return postTranslateContent.ToString();
        }

        /// <summary>
        /// Extracts AFRM tooltip tags and their content into a dictionary.
        /// </summary>
        private static Dictionary<string, string> ConvAfrmTooltipPreTextToDict(string orgText)
        {
            var dict = new Dictionary<string, string>();
            foreach (string tag in new[] { "name", "desc", "pdesc", "autodesc", "hdesc", "descleftalign" })
            {
                string startTag = $"<{tag}>";
                string endTag = $"</{tag}>";
                if (!orgText.Contains(startTag))
                    continue;

                int tagIndex = orgText.IndexOf(startTag) + startTag.Length;
                int endIndex = orgText.IndexOf(endTag);
                dict[tag] = orgText.Substring(tagIndex, endIndex - tagIndex);
            }
            return dict;
        }

        /// <summary>
        /// Waits for the glossary CSV file to be released (not in use by another process).
        /// </summary>
        public static void WaitingForGlossaryTableRelease()
        {
            string glossaryPath = GetGlossaryTableFullPath();
            if (!File.Exists(glossaryPath))
                return;

            bool fileOccupied = true;
            while (fileOccupied)
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(glossaryPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    fileOccupied = false;
                }
                catch
                {
                    MessageBoxEx.Show("請關閉Glossary.csv編輯。\r\n確認關閉後，請點OK。", "注意");
                }
                finally
                {
                    fs?.Close();
                }
            }
        }

        #endregion

        #region Cache Management

        /// <summary>
        /// Attempts to fetch a cached translation result for the given text.
        /// </summary>
        private static string TryFetchCachedTranslationResult(string orgText)
        {
            string cachePath = GetEngineCachePath(DefaultPreferredTranslateEngine, DefaultDesiredLanguage);

            if (!File.Exists(cachePath))
                return "";

            try
            {
                var currentTranslationCache = JObject.Parse(File.ReadAllText(cachePath));
                string checksum = GetSha256Checksum(orgText);
                var token = currentTranslationCache.SelectToken($"['{checksum}']");
                return token?.ToString() ?? "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Caches a translation result for future use.
        /// </summary>
        private static void CacheTranslationResult(string orgText, string translatedText)
        {
            // Don't cache if text hasn't changed
            if (orgText.Contains("\r\n") && orgText == translatedText)
                return;

            string cachePath = GetEngineCachePath(DefaultPreferredTranslateEngine, DefaultDesiredLanguage);
            var currentTranslationCache = new JObject();

            try
            {
                if (File.Exists(cachePath))
                {
                    string content = File.ReadAllText(cachePath);
                    if (!string.IsNullOrEmpty(content))
                        currentTranslationCache = JObject.Parse(content);
                }

                string checksum = GetSha256Checksum(orgText);
                currentTranslationCache[checksum] = translatedText;
                
                Directory.CreateDirectory(Path.GetDirectoryName(cachePath));
                File.WriteAllText(cachePath, currentTranslationCache.ToString());
            }
            catch (Exception ex)
            {
                LogTranslationError("CacheTranslationResult", orgText, ex);
            }
        }

        /// <summary>
        /// Initializes all cache directories for translation engines.
        /// </summary>
        public static void InitializeCache()
        {
            var cacheDirs = new[]
            {
                "google",
                "mozhi-google",
                "mozhi-deepl",
                "mozhi-duckduckgobing",
                "mozhi-mymemory",
                "mozhi-yandex",
                "naver",
                "openai"
            };

            string basePath = GetTranslationCacheBasePath();
            foreach (string dir in cacheDirs)
            {
                string createPath = Path.Combine(basePath, dir);
                if (!Directory.Exists(createPath))
                {
                    Directory.CreateDirectory(createPath);
                }
            }
        }

        #endregion

        #region Glossary Management

        /// <summary>
        /// Checks if a glossary table is available for use.
        /// </summary>
        private static bool UseGlossaryTable()
        {
            return File.Exists(GetGlossaryTableFullPath());
        }

        /// <summary>
        /// Loads glossary data and creates a mapping from terms to identifiers.
        /// </summary>
        private static Dictionary<string, string> GlossaryToIdentifier()
        {
            var glossary = new Dictionary<string, string>();
            
            if (!UseGlossaryTable())
                return glossary;

            try
            {
                using (var reader = new StreamReader(GetGlossaryTableFullPath()))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    csv.Read();
                    csv.ReadHeader();
                    
                    while (csv.Read())
                    {
                        string identifier = csv.GetField("identifier");
                        foreach (var langCode in new[] { "ko", "ja", "zh-CN", "zh-TW", "en" })
                        {
                            string word = csv.GetField(langCode);
                            if (!string.IsNullOrEmpty(word))
                            {
                                glossary[word] = identifier;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogTranslationError("GlossaryToIdentifier", "", ex);
            }

            return glossary;
        }

        /// <summary>
        /// Loads glossary data and creates a mapping from identifiers to terms in the specified language.
        /// </summary>
        private static Dictionary<string, string> IdentifierToGlossary(string langcode)
        {
            var glossary = new Dictionary<string, string>();
            
            if (!UseGlossaryTable())
                return glossary;

            try
            {
                using (var reader = new StreamReader(GetGlossaryTableFullPath()))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    csv.Read();
                    csv.ReadHeader();
                    
                    while (csv.Read())
                    {
                        string identifier = csv.GetField("identifier");
                        string word = csv.GetField(langcode);
                        if (!string.IsNullOrEmpty(word))
                        {
                            glossary[identifier] = word;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogTranslationError("IdentifierToGlossary", "", ex);
            }

            return glossary;
        }

        /// <summary>
        /// Preprocesses text by replacing glossary terms with identifiers before translation.
        /// </summary>
        private static string GlossaryPreProcess(string orgText)
        {
            if (!UseGlossaryTable())
                return orgText;

            string processedText = orgText;
            foreach (var pair in GlossaryToIdentifier())
            {
                processedText = Regex.Replace(processedText, Regex.Escape(pair.Key), pair.Value, RegexOptions.IgnoreCase);
            }

            return processedText;
        }

        /// <summary>
        /// Post-processes text by replacing identifiers back with glossary terms in the target language.
        /// </summary>
        private static string GlossaryPostProcess(string postText, string langcode)
        {
            if (!UseGlossaryTable())
                return postText;

            string processedText = postText;
            var glossary = IdentifierToGlossary(langcode);

            foreach (var pair in glossary)
            {
                string pattern = Regex.Escape(pair.Key);
                string replacement = pair.Value + (langcode == "en" || langcode == "ko" ? " " : "");
                processedText = Regex.Replace(processedText, pattern, replacement, RegexOptions.IgnoreCase);

                // Workarounds for translation engine faults
                string altPattern = Regex.Escape(pair.Key.Replace("_0", "_"));
                processedText = Regex.Replace(processedText, altPattern, replacement, RegexOptions.IgnoreCase);

                string brokenPattern = Regex.Escape(pair.Key.Replace("<", "</"));
                processedText = Regex.Replace(processedText, brokenPattern, "", RegexOptions.IgnoreCase);
            }

            // Clean up extra spaces
            if (langcode == "en" || langcode == "ko")
                processedText = processedText.Replace("  ", " ");

            return processedText;
        }

        #endregion

        #region Currency Conversion

        /// <summary>
        /// Converts a point value to the desired currency based on the source language.
        /// </summary>
        public static string GetConvertedCurrency(int pointValue, string sourceLanguage)
        {
            if (DefaultDesiredCurrency == "none")
                return null;

            UpdateExchangeTable();

            if (string.IsNullOrEmpty(ExchangeTable))
                return null;

            double irlPrice = ConvertPointsToCurrency(pointValue, sourceLanguage);
            var exTable = JObject.Parse(ExchangeTable);

            string sourceCurrency = DefaultDetectCurrency == "auto"
                ? LanguageToCurrency.TryGetValue(sourceLanguage, out var cur) ? cur : sourceLanguage
                : DefaultDetectCurrency;

            double exchangeMultiplier = 1.0;
            double.TryParse(exTable.SelectToken($"{DefaultDesiredCurrency}.{sourceCurrency}")?.ToString() ?? "1", out exchangeMultiplier);

            double convertedPrice = irlPrice / exchangeMultiplier;

            return DefaultDesiredCurrency switch
            {
                "jpy" or "krw" => $"約{Math.Round(convertedPrice)}{GetCurrencySymbol(DefaultDesiredCurrency)}",
                _ => $"約{convertedPrice:0.##}{GetCurrencySymbol(DefaultDesiredCurrency)}"
            };
        }

        /// <summary>
        /// Converts point values to real currency based on source language conventions.
        /// </summary>
        private static double ConvertPointsToCurrency(int pointValue, string sourceLanguage)
        {
            return sourceLanguage switch
            {
                "zh-CN" => pointValue / 100.0,  // CMS: 100 points = 1 CNY
                "en" => pointValue / 1000.0,    // GMS: 1000 points = 1 USD
                _ => pointValue
            };
        }

        /// <summary>
        /// Converts a currency code to its corresponding language code.
        /// </summary>
        public static string ConvertCurrencyToLang(string currency)
        {
            return CurrencyToLanguage.TryGetValue(currency, out var lang) ? lang : null;
        }

        /// <summary>
        /// Checks if the given text is in the desired language.
        /// </summary>
        public static bool IsDesiredLanguage(string orgText)
        {
            if (string.IsNullOrEmpty(orgText))
                return true;

            string detectedLang = GTranslateDetect(orgText, DefaultDesiredLanguage);
            return detectedLang == DefaultDesiredLanguage;
        }

        /// <summary>
        /// Fetches and updates the exchange rate table from remote sources.
        /// </summary>
        public static void UpdateExchangeTable()
        {
            if (!string.IsNullOrEmpty(ExchangeTable))
                return;

            foreach (string baseUrl in CurrencyBaseURLs)
            {
                string fetchURL = baseUrl + DefaultDesiredCurrency + ".min.json";
                try
                {
                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, fetchURL);
                    requestMessage.Headers.Accept.ParseAdd("application/json");
                    var response = _httpClient.SendAsync(requestMessage).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        ExchangeTable = response.Content.ReadAsStringAsync().Result;
                        return;
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Computes the SHA256 checksum of the given input string.
        /// </summary>
        private static string GetSha256Checksum(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Loads a dictionary from a JSON file.
        /// </summary>
        public static Dictionary<string, string> LoadDict(string jsonFilePath)
        {
            var result = new Dictionary<string, string>();
            
            if (!File.Exists(jsonFilePath))
                return result;

            try
            {
                var jsonObj = JObject.Parse(File.ReadAllText(jsonFilePath));
                foreach (var pair in jsonObj)
                {
                    result[pair.Key] = pair.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                LogTranslationError("LoadDict", jsonFilePath, ex);
            }

            return result;
        }

        /// <summary>
        /// Saves a dictionary to a JSON file.
        /// </summary>
        public static void SaveDict(string jsonFilePath, Dictionary<string, string> dict)
        {
            try
            {
                var jsonObj = new JObject();
                foreach (var pair in dict)
                {
                    jsonObj[pair.Key] = pair.Value;
                }
                
                Directory.CreateDirectory(Path.GetDirectoryName(jsonFilePath));
                File.WriteAllText(jsonFilePath, jsonObj.ToString());
            }
            catch (Exception ex)
            {
                LogTranslationError("SaveDict", jsonFilePath, ex);
            }
        }

        /// <summary>
        /// Backward compatibility wrapper for loadDict (lowercase).
        /// </summary>
        public static Dictionary<string, string> loadDict(string jsonFilePath) => LoadDict(jsonFilePath);

        /// <summary>
        /// Backward compatibility wrapper for saveDict (lowercase).
        /// </summary>
        public static void saveDict(string jsonFilePath, Dictionary<string, string> dict) => SaveDict(jsonFilePath, dict);

        #endregion

        #region Global Settings

        public static string ExchangeTable { get; set; }
        public static string DefaultDesiredLanguage { get; set; }
        public static string DefaultMozhiBackend { get; set; }
        public static string DefaultLanguageModel { get; set; }
        public static string DefaultTranslateAPIKey { get; set; }
        public static string DefaultOpenAISystemMessage { get; set; }
        public static int DefaultPreferredLayout { get; set; }
        public static int DefaultPreferredTranslateEngine { get; set; }
        public static bool IsTranslateEnabled { get; set; }
        public static string DefaultDetectCurrency { get; set; }
        public static string DefaultDesiredCurrency { get; set; }
        public static double DefaultLMTemperature { get; set; }
        public static int DefaultMaximumToken { get; set; }
        public static bool IsExtraParamEnabled { get; set; }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Represents a skill record with ID and name.
    /// </summary>
    public class SkillRecord
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// CsvHelper mapping class for SkillRecord.
    /// </summary>
    public sealed class RecordMap : ClassMap<SkillRecord>
    {
        public RecordMap()
        {
            Map(m => m.ID).Name("skillID");
            Map(m => m.Name).Name("skillName");
        }
    }

    /// <summary>
    /// Provides CSV lookup functionality for ID-to-name mappings.
    /// </summary>
    public class CsvLookup
    {
        private readonly Dictionary<int, string> _idNameDict;

        public CsvLookup(string csvPath)
        {
            _idNameDict = new Dictionary<int, string>();

            using (var reader = new StreamReader(csvPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<RecordMap>();
                var records = csv.GetRecords<SkillRecord>();

                foreach (var record in records)
                {
                    if (!_idNameDict.ContainsKey(record.ID))
                    {
                        _idNameDict[record.ID] = record.Name;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the name for the given ID, or null if not found.
        /// </summary>
        public string GetNameById(int id)
        {
            return _idNameDict.TryGetValue(id, out var name) ? name : null;
        }
    }

    #endregion
}
