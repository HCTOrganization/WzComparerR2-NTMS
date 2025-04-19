[WzComparerR2-JMSをダウンロード](https://github.com/HikariCalyx/WzComparerR2-JMS/releases)

# WzComparerR2-JMS
- これは、JMS 用に設計されたメイプルストーリー抽出ツールです。
- KMS、GMS、CMS などの他のクライアントで動作します。
- このツールは、WZ ファイルの編集をサポートしていません。

# Modules
- **WzComparerR2** 主程序
- **WzComparerR2.Common** 一些通用类
- **WzComparerR2.PluginBase** 插件管理器
- **WzComparerR2.WzLib** wz文件读取相关
- **CharaSimResource** 用于装备模拟的资源文件
- **WzComparerR2.LuaConsole** (可选插件)Lua控制台
- **WzComparerR2.MapRender** (可选插件)地图仿真器
- **WzComparerR2.Avatar** (可选插件)纸娃娃
- **WzComparerR2.Network** (可选插件)在线聊天室

# Usage
- **2.x**: Win7+/.net4.8+/dx11.0

# 翻訳機能
- WzComparerR2-JMS v5.6.0 以降では、翻訳機能が導入されました。
- 以下の翻訳エンジンと連携します: Google、DeepL、DuckDuckGo/Bing、MyMemory、Yandex、Naver Papago。

### Mozhiサーバー
Mozhi は、公開されている API を備えた、多くの翻訳エンジンの代替フロントエンドです。[Mozhi プロジェクトの詳細については、こちらをご覧ください。](https://mozhi.aryak.me/about)

### Naver Papago
韓国語のテキストの翻訳に関しては、Naver Papago はすべての翻訳エンジンの中で比較的最高の結果を達成しています。ただし、Naver Papago を使用するには API キーが必要です。[API キーはここからリクエストできます。](https://guide.ncloud-docs.com/docs/ja/papagotranslation-api)

API キーを取得したら、次のように JSON 形式で「翻訳APIキー」テキストボックスに入力してください:
```
{
    "X-NCP-APIGW-API-KEY-ID": "APIキーIDに置き換えてください",
    "X-NCP-APIGW-API-KEY": "APIキーに置き換えてください"
]
```

# NX OpenAPI
- [API キーを取得する方法については、こちらをご覧ください。](https://openapi.nexon.com/guide/prepare-in-advance/)
- 他の国や地域のNX IDは使用できません。韓国のNX IDのみ使用できます。
- [OpenAPI 機能の詳細については、こちらをご覧ください。](https://openapi.nexon.com/game/maplestory/)

### ItemID to NX OpenAPI ItemIcon Filename
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

たとえば、次の ItemIcon URL はアイテム ID 1802767 を表します。非 KMS アイテムは利用できません。
```
https://open.api.nexon.com/static/maplestory/ItemIcon/KEHCJAIG.png
```

# About Kinoko Game (キノコゲーム) Section

Since JMS v427 the old patch server is abandoned. Now the game can be only run with Nexon Game Manager. To make the entire procedure easier for new players, Download Game (ゲームをダウンロード) and Game Start (ゲームスタート) buttons were added as of WzComparerR2-JMS v5.5.0.

If you see message says You'll need a new app to open this ngm link, please download Nexon Game Manager from either [KMS official website](https://maplestory.nexon.com/Common/PDS/Download) or [JMS official website](https://maplestory.nexon.co.jp).

# コンパイル
- GitHub Desktop を使用してこのリポジトリをクローンします。
- [Visual Studio 2022 Community](https://visualstudio.microsoft.com/downloads/) を使用して WzComparerR2.sln を開きます。
- [ビルド] - [ソリューションのビルド] を選択してコンパイルします。
- ビルドは WzComparerR2\bin\Release ディレクトリにあります。

# 問題の報告
- WzComparerR2-JMS で問題が見つかった場合は、[Kagamiaのビルド](https://github.com/Kagamia/WzComparerR2/releases/latest)で問題が再現できるかどうかを確認してください。Kagamia のビルドで再現可能な場合は、そのリポジトリで問題を作成してください。そうでない場合は、このリポジトリで問題を作成できます。


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
