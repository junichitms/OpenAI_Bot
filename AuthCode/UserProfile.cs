using System;

namespace OpenAI_Bot
{
    public class UserProfile
    {
        public int count { get; set; }
        public int count2 { get; set; }
        public string Authtoken { get; set; }
        public DateTime date { get; set; }
        //結果を格納
        public result[] results { get; set; }
        //保持する会話履歴
        public Conversation_History[] Conversation_Historys { get; set; }
    }
    public class result
    {
        public int CompletionTokens { get; set; }
        public int PromptTokens { get; set; }
        public int TotalTokens { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }
    public class Conversation_History
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
