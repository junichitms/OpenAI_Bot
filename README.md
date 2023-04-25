# Bot Frameworkで、Azure OpenAI Serviceを利用するアプリです
## あえて、c#で構築しています。(c#で、Azure OpenAIを利用した例があまりないので)

このデモサンプルでは、Azure OpenAI Serviceのc# SDKを使っています。(Beta版です)
NuGet から Azure.AI.OpenAI をインストールすれば、使えます。
(ChatCompletionを使ってます)
- Azure.AI.OpenAI ([here](https://www.nuget.org/packages/Azure.AI.OpenAI/1.0.0-beta.5))

また、Bot Frameworkのステート情報を、ストレージに保存するための機能を使って、Cosmos DB(Core SQL API)に、
Azure OpenAI Serviceとやり取りした内容や使ったトークン数を履歴として残します。

## 構築手順

> **重要:** デモサンプルをデプロイして実行するには、 **Azure OpenAI Service のアクセスを有効にした** Azure サブスクリプションが必要です。[ここ](https://aka.ms/oaiapply)からリクエストできます。

### 利用環境
- Visual Studio 2022
- .NET SDK version 6.0
- Azure App Serviceを作成。(.NET6 / Windows環境)　デプロイ時に作成しても可。
- Azure OpenAI Serviceを作成し、キーとエンドポイントから[キー1]と[エンドポイント]をメモしておきます。
![OpenAI screen](docs/AOAI.jpg)
次に、モデルデプロイから、+作成をクリックし　gpt-35-turboをデプロイします。[モデルデプロイ名] をメモしておきます。
![OpenAI screen](docs/AOAI1.jpg)
- Azure Cosmos DB(Core SQL AOI)を作成し、データベースを作成。
データエクスプローラーから[New Database]で、作成しデータベース名をメモしておきます。
![CosmosDB screen](docs/cosmos1.jpg)
次に、キーから[URI]と[プライマリキー]をメモしておきます。
![CosmosDB screen](docs/cosmos2.jpg)
- Azure Botを作成します。
![Bot screen](docs/bot1.jpg)
構成から、[ボットタイプ] , [アプリテナントID] ,[Microsoft App ID]をメモしておきます。
次に、Microsft App ID(パスワードの管理)から、パスワードの管理をクリックします。
![Bot screen](docs/bot2.jpg)
+新しいクライアントシークレットをクリックし、シークレットを追加し[値]をメモしておきます。
![Bot screen](docs/bot3.jpg)
Azure Botのチャネルから、Direct Lineをクリックします。
![Bot screen](docs/bot6.jpg)
Default Siteをクリックし、[秘密キー]をメモしておきます。(上段下段どちらでも可)
![Bot screen](docs/bot7.jpg)


### 構築

1. リポジトリをダウンロードもしくは、`git clone` します。
1. Visual Studioから、OpenAI_Bot.slnを開きます。
1. appsettings.json開き、値をセットします。。
    ```
    MicrosoftAppType        :  メモしたAzure Botの[ボットタイプ] 
    MicrosoftAppId          :  メモしたAzure Botの[Microsoft App ID]
    MicrosoftAppPassword    :  メモしたAzure Botのシークレットの[値]
    MicrosoftAppTenantId    :  メモしたAzure Botの[アプリテナントID]
    Welcome                 :  Botを起動した際のウェルカムメッセージを入力します。例: このボットは、Azure Open AI Servicesを利用したデモです。\nChat Completionを利用。\n言語モデルは、gpt-35-turboの利用を想定しています\n\n会話履歴を保持していますが、resetと入力することで、会話履歴をリセットできます。
    OpenAIEndpoint          :  メモしたAzure OpenAI Serviceの[エンドポイント]
    OpenAIKey               :  メモしたAzure OpenAI Serviceの[キー1] 
    OpenAIModel             :  メモしたAzure OpenAI Serviceの[モデルデプロイ名] 
    MaxTokens               :  ChatGPTのパラメータ Max Tokensを指定します。例 : 800
    Temperature             :  ChatGPTのパラメータ Temeratureを指定します。例 : 0.7
    SystemMessage           :  ChatGPTのSystem メッセージを入力します。アシスタントの簡単な説明 , 性格的な特性 , 従ってもらいたいルールを指定することが可能。 例 : あなたは寿司の職人です。与えられた名前の寿司を美味しそうに解説してください。寿司ではない名前を言われた場合は解説は行わずに「寿司以外聞かないで。」と答えてください。
    CosmosDbEndpoint        :  メモしたAzure Cosmos DBの[URI]
    CosmosDbAuthKey         :  メモしたAzure Cosmos DBの[プライマリキー]
    CosmosDbDatabaseId      :  メモしたAzure Cosmos DBのデータベース名
    ContainerId             :  Cosmos DBのコンテナ名。利用環境で作っていなくても、実行時に作成されます。例: BotState
    ```
1. wwwrootからdefault.htmを開きます。メモしたAzure Bot　Direct Lineの[秘密キー]を、token: にセットします。
![VS screen](docs/vs1.jpg)
1. Azure App Serviceにデプロイします。以下は、Visual Studioからの発行例です。
![VS screen](docs/vs.jpg)
1. Azure Portalの Azure Botの画面から、構成を開きメッセージングエンドポイントに以下をセットします。
     
    App ServiceにデプロイしたURL/api/messages
    ![Bot screen](docs/bot5.jpg)

Webチャットでテストから、動作するか確認できます。
![Cosmos screen](docs/bot8.jpg)


### Web Chat画面

* App ServiceにデプロイしたURLをブラウザで開いて下さい。
このBotは、会話履歴を保持し、会話を継続していきます。履歴をリセットしたい場合は、resetを入力すると履歴がリセットされます。(リセットしないで、継続するとトークン数がオーバーする可能性もあるので、適宜resetして下さい。)
上記のSystemメッセージを変更して、どのようなアシスタントとしてボットを提供するかいろいろと試して下さい。
また、default.htmやcssを変更することで、画面をカスタマイズ可能です。
![Bot screen](docs/web.jpg)


### 履歴
Azure OpenAI Serviceとのやり取りや使用したトークン数はCosmos DBから参照できます。
Bot Frameworkを使うことで、Cosmos DBを宣言するだけで履歴を残すことができるメリットがあります。
![Bot screen](docs/cosmos3.jpg)

### その他のチャネルからの利用
Azure Portalの Azure Botの画面から、チャネルを開き、有効にすることで様々なチャネルに対応することが可能です。
![Bot screen](docs/ch.jpg)

### Botに認証を追加する方法
Azure Active Directory (Azure AD) v2 を ID プロバイダーとして使用して、ボットの OAuth トークンを取得後、Microsoft Graphを使いUser.DisplayNameを取得し、表示します。
1. 以下のLinkを参照し、[Azure AD ID プロバイダーを作成する]を実施します。
    * 認証を追加する ([here](https://learn.microsoft.com/ja-jp/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=multitenant%2Caadv2%2Ccsharp#create-the-azure-ad-identity-provider))
    * 次に、上記のLinkの[Azure AD ID プロバイダーをボットに登録する]を実施します。サービス プロバイダーは、 [Azure Active Directory v2]にし、スコープで、User.Read User.ReadBasic.Allも設定します。設定した[名前]をメモしておきます。
1. Visual Studioから、OpenAI_Bot.slnを開きます。
    * NuGet から、Microsoft.Graphをインストールします。
    * /botsOpenAIBot.cs/OpenAIBot.csを、AuthCodeにあるOpenAIBot.csに置き換えます。
    * /Dialogs/OpenAIBot.csを、AuthCodeにあるOpenAIDialog.csに置き換えます。
    * UserProfile.csを、AuthCodeにあるUserProfile.csに置き換えます。
    * AuthCodeにあるappsettings.jsonを参考にし、既存のappsettings.jsonへ、 "ConnectionName": "メモした名前を設定"を追加します。
1. ビルドして、App Serviceへ再度デプロイします。
1. これで、認証を追加できました。以下のように動作します。
![BotAuth screen](docs/auth1.jpg)
![BotAuth screen](docs/auth2.jpg)
![BotAuth screen](docs/auth3.jpg)

### 免責
こちらは、デモサンプルの為、何の保証も提供しておりませんのでご了承ください。
