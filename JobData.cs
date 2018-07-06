using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore_QnA_Bot
{
    /// <summary>
    /// Class for storing job state. 
    /// </summary>
    public class JobData
{
    /// <summary>
    /// The name to use to read and write this bot state object to storage.
    /// </summary>
    public readonly static string PropertyName = $"BotState:{typeof(Dictionary<int, JobData>).FullName}";

    public int JobNumber { get; set; } = 0;
    public bool Completed { get; set; } = false;

    /// <summary>
    /// The conversation reference to which to send status updates.
    /// </summary>
    public ConversationReference Conversation { get; set; }
}
}



