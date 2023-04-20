using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Azure.AI.OpenAI;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace OpenAI_Bot.Dialogs
{
    public class OpenAIDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        private string _openaiendpoint;
        private string _opneaikey;
        private string _openaimodel;
        private int _openaimaxtokens;
        private float _openaitemperature;
        private string _openaisystemmessage;
        public OpenAIDialog(UserState userState, IConfiguration config)
            : base(nameof(OpenAIDialog))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");
            _openaiendpoint = config["OpenAIEndpoint"];
            _opneaikey = config["OpenAIKey"];
            _openaimodel = config["OpenAIModel"];
            _openaimaxtokens = Int32.Parse(config["MaxTokens"]);
            _openaitemperature = float.Parse(config["Temperature"]);
            _openaisystemmessage = config["SystemMessage"];

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
           {
                qstep,
                astep,
           };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));


            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> qstep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            //resetと入力されたら、会話履歴をれセットし、Dialogをキャンセル。
            if ((string)stepContext.Context.Activity.Text == "reset")
            {
                userProfile.Conversation_Historys = null;
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("会話履歴をリセットしました。再度プロンプトを入力して下さい。"), cancellationToken);
                return await stepContext.CancelAllDialogsAsync(cancellationToken: cancellationToken);
            }

            //Azure OpenAI Servicesとのやり取りを保存するプロファイルのリストを作成
            var list = new List<result>();
            if (userProfile.results == null)
            {
                userProfile.count = 0;
                var ret = new result();
                list.Add(ret);
            }
            else
            {
                list.AddRange(userProfile.results);
                userProfile.count = list.Count;
                var ret = new result();
                list.Add(ret);
            }
            //Azure OpenAI Servicesとの会話履歴を保存するプロファイルのリストを作成
            var list2 = new List<Conversation_History>();
            if (userProfile.Conversation_Historys == null)
            {
                userProfile.count2 = 0;
                var ret2 = new Conversation_History();
                list2.Add(ret2);
            }
            else
            {
                list2.AddRange(userProfile.Conversation_Historys);
                userProfile.count2 = list2.Count;
                var ret2 = new Conversation_History();
                list2.Add(ret2);
            }

            //Azure OpenAI Servicesとのやり取りを保存するプロファイルのリストをセット。
            userProfile.results = list.ToArray();
            userProfile.results[userProfile.count].Question = (string)stepContext.Context.Activity.Text;
            //Azure OpenAI Servicesとの会話履歴を保存するプロファイルのリストをセット。
            userProfile.Conversation_Historys = list2.ToArray();
            userProfile.Conversation_Historys[userProfile.count2].Question = (string)stepContext.Context.Activity.Text;
            //変更日付をセット
            userProfile.date = DateTime.Now;

            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> astep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            int i = userProfile.count;
            int j = userProfile.count2;

            //Azure OpenAIのクライアントを作成
            OpenAIClient openaicl = new OpenAIClient(new Uri(_openaiendpoint), new Azure.AzureKeyCredential(_opneaikey));
            //chat completion optionを作成
            var chatcompletionop = new ChatCompletionsOptions();
            //Systemメッセージをセット。アシスタントの簡単な説明 , 性格的な特性 , 従ってもらいたいルールを指定することが可能。
            chatcompletionop.Messages.Add(new ChatMessage(ChatRole.System, _openaisystemmessage));
            //保存している会話履歴をユーザー/アシスタントとしてセット。qstepで、userProfile.Conversation_Historysは、最後ユーザーのQuestionになり、アシスタントはならない。
            foreach (var ret in userProfile.Conversation_Historys)
            {
                chatcompletionop.Messages.Add(new ChatMessage(ChatRole.User, ret.Question));
                if (ret.Answer != null)
                {
                    chatcompletionop.Messages.Add(new ChatMessage(ChatRole.Assistant, ret.Answer));
                }

            }
            //Max Tokensの設定
            chatcompletionop.MaxTokens = _openaimaxtokens;
            //Temperatureの設定
            chatcompletionop.Temperature = _openaitemperature;
            //Azure OpenAIへ、Chat Complettionを実行
            var result = await openaicl.GetChatCompletionsAsync(_openaimodel, chatcompletionop);
            //実行結果を、やり取りを保存するプロファイルにセット。
            userProfile.results[i].Answer = result.Value.Choices[0].Message.Content;
            userProfile.results[i].PromptTokens = result.Value.Usage.PromptTokens;
            userProfile.results[i].CompletionTokens = result.Value.Usage.CompletionTokens;
            userProfile.results[i].TotalTokens = result.Value.Usage.TotalTokens;
            //実行結果の返信を会話履歴のAnswerにセット。
            userProfile.Conversation_Historys[j].Answer = result.Value.Choices[0].Message.Content;


            await stepContext.Context.SendActivityAsync(MessageFactory.Text(result.Value.Choices[0].Message.Content), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
