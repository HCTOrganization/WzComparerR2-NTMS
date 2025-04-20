[下載WzComparerR2-NTMS](https://github.com/HCTOrganization/WzComparerR2-NTMS/releases)

# WzComparerR2-NTMS
- 這是專為TMS設計的新楓之谷資料提取工具。
- 也支援其他官方客戶端（如 KMS、GMS、CMS）。
- 此工具不支援編輯WZ檔。

# 模組
- **WzComparerR2** 主程式
- **WzComparerR2.Common** 一些通用類
- **WzComparerR2.PluginBase** 外掛程式管理器
- **WzComparerR2.WzLib** wz檔案讀取相關
- **CharaSimResource** 用於裝備模擬的資源文件
- **WzComparerR2.LuaConsole** (可選插件)Lua控制台
- **WzComparerR2.MapRender** (選用外掛)地圖模擬器
- **WzComparerR2.Avatar** (可選插件)紙娃娃
- **WzComparerR2.Network** (可選外掛)線上聊天室

＃ 用法
- **2.x**: Win7+/.net4.8+/dx11.0

# 翻譯功能
- WzComparerR2-JMS v5.6.0 及更高版本中引入了翻譯功能。
- 可與以下翻譯引擎搭配使用：Google、DeepL、DuckDuckGo/Bing、MyMemory、Yandex、Naver Papago。

### Mozhi 伺服器
Mozhi 是許多翻譯引擎的替代前端，具有公開可用的 API。 [有關 Mozhi 項目的更多資訊可以在這裡找到。 ]]（https://mozhi.aryak.me/about）

### Naver Papago
在翻譯韓文文本時，Naver Papago 在所有翻譯引擎中都取得了相對較好的效果。但是，您需要一個 API 金鑰才能使用 Naver Papago。 [您可以在此處請求 API 金鑰。 ](https://guide.ncloud-docs.com/docs/ja/papagotranslation-api)

取得 API 金鑰後，請將其以 JSON 格式輸入至「翻譯 API 金鑰」文字方塊中，如下所示：
```
{
"X-NCP-APIGW-API-KEY-ID": "替換為您的 API 密鑰 ID",
"X-NCP-APIGW-API-KEY": "替換為您的 API 金鑰"
]
```

# NX 開放 API
- [在此處了解如何取得 API 金鑰。 ](https://openapi.nexon.com/guide/prepare-in-advance/)
- 無法使用其他國家或地區的 NX ID。僅可使用韓國 NX ID。
- [有關 OpenAPI 功能的更多信息，請參見此處。 ](https://openapi.nexon.com/game/maplestory/)

### ItemID 到 NX OpenAPI ItemIcon 檔名
|   |1st |2nd |3rd |4th |5th |6th |7th |
|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
|0  |    |P   |C   |L   |H   |O   |B   |
|1  |E   |O   |D   |A   |G   |P   |A   |
|2  |H   |N   |A   |J   |F   |M   |D   |
|3  |G   |M   |B   |I   |E   |N   |C   |
|4  |B   |L   |G   |P   |D   |K   |F   |
|5  |A   |K   |H   |O   |C   |L   |E   |
|6  |    |J   |E   |N   |B   |I   |H   |
|7  |    |I   |F   |M   |A   |J   |G   |
|8  |    |H   |K   |D   |P   |G   |J   |
|9  |    |G   |I   |C   |O   |H   |I   |

例如，以下 ItemIcon URL 代表道具ID 1802767：非 KMS 道具不可用。

```
https://open.api.nexon.com/static/maplestory/ItemIcon/KEHCJAIG.png
```

# 編譯
- 使用 GitHub Desktop 複製此儲存庫。
- 使用 [Visual Studio 2022 Community](https://visualstudio.microsoft.com/downloads/)開啟 WzComparerR2.sln。
- 選擇 Build - Build Solution 進行編譯。
- 此版本位於 WzComparerR2\bin\Release 目錄中。

# 回報問題
- 如果您發現 WzComparerR2-NTMS 有問題，請嘗試在您的 [Kagamia 版本](https://github.com/Kagamia/WzComparerR2/releases/latest) 中重現問題。如果它可以在 Kagamia 版本中重現，請在該儲存庫中建立問題。如果沒有，您可以在 WzComparerR2-JMS 儲存庫中建立一個 issue。


# Credits
- **Fiel** ([Southperry](http://www.southperry.net))  wz文件读取代码改造自WzExtract 以及WzPatcher
- **Index** ([Exrpg](http://bbs.exrpg.com/space-uid-137285.html)) MapRender的原始代码 以及libgif
- **Deneo** For .ms file format and video format
- **[DotNetBar](http://www.devcomponents.com/)**
- **[IMEHelper](https://github.com/JLChnToZ/IMEHelper)**
- **[Spine-Runtime](https://github.com/EsotericSoftware/spine-runtimes)**
- **[EmptyKeysUI](https://github.com/EmptyKeys)**
- **[libvpx](https://www.webmproject.org/code/) & [libyuv](https://chromium.googlesource.com/libyuv/libyuv/)** for video decoding
- **[VC-LTL5](https://github.com/Chuyu-Team/VC-LTL5)** for native library build
- **[@KENNYSOFT](https://github.com/KENNYSOFT)** and his WcR2-KMS version.
- **[@Kagamia](https://github.com/Kagamia)** and her WcR2-CMS version.
- **[@Spadow](https://github.com/Sunaries)** for providing his WcR2-GMS version.
- **[@PirateIzzy](https://github.comPirateIzzy)** for providing the basis of this fork.
- **[@seotbeo](https://github.com/seotbeo)** for providing Skill comparison feature.
