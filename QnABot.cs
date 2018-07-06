// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AspNetCore_QnA_Bot
{
    public class QnABot : IBot
    {

        private readonly DialogSet dialogs;


        public QnABot()
        {

            dialogs = new DialogSet();

                dialogs.Add("greetings", new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        var activity = MessageFactory.Attachment(
                            new HeroCard(
                                title: "Hi! I am the Q Bot",
                                text: " I can help you with the following areas, unit details, " +
                                "assessments, learning resources and study and administrative questions. " +
                                "I'm also going to be prompting you throughout the course of the semester " +
                                "to help keep you on track. Feel free to ask me a question or type 'Help' " +
                                "at any time to get a list of common interactions.",
                                images: new CardImage[] { new CardImage(url: "https://qutdemodotnetv4a6a4.blob.core.windows.net/files/Capture.PNG") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "try proactive", type: ActionTypes.ImBack, value: "proactive")
                                    })
                            .ToAttachment());

                    await dc.Context.SendActivity(activity);
                    await dc.End();

                    }
                });
            dialogs.Add("textPrompt", new Microsoft.Bot.Builder.Dialogs.TextPrompt());


            dialogs.Add("help", new WaterfallStep[]
            {
                    async (dc, args, next) =>
                    {
                        await dc.Context.SendActivity("Help Dialog Message");
                        await dc.End();
                    }
             });
        }



        public async Task OnTurn(ITurnContext context)
        {
            var state = ConversationState<Dictionary<string, object>>.Get(context);
            var state2 = context.GetConversationState<State>();
        //    state2.name = "Valeriia";

            var dc = dialogs.CreateContext(context, state);

            if (context.Activity.Type == ActivityTypes.Message)
            {
                await dc.Continue();

                if (!context.Responded)
                {
                    if (context.Activity.Text.ToLowerInvariant().Contains("proactive"))
                    {
                        var jobLog = GetJobLog(context);
                        var job = CreateJob(context, jobLog);
                        var appId = "f68cf47c-cbdd-4f23-9eda-71cca9dc4632";
                        var conversation = TurnContext.GetConversationReference(context.Activity);
                        await context.SendActivity($"We're starting job {job.JobNumber} for you. We'll notify you when it's complete.");



                        var adapter = context.Adapter;
                        await Task.Run(() =>
                         {
                            Thread.Sleep(5000);

                            // Perform bookkeeping and send the proactive message.
                            CompleteJob(adapter, appId, conversation, job.JobNumber);
                         });

                    } else {

                        await dc.Begin("greetings");
                    }
                }
            }
        }




        private static Random NumberGenerator = new Random();

        private static Dictionary<int, JobData> GetJobLog(ITurnContext context)
        {
            return context.Services.Get<Dictionary<int, JobData>>(JobData.PropertyName);
        }

        private JobData CreateJob(ITurnContext context, Dictionary<int, JobData> jobLog)
        {
            int number = 5;
            var job = new JobData
            {
                JobNumber = number,
                Conversation = TurnContext.GetConversationReference(context.Activity)
            };
            jobLog.Add(job.JobNumber, job);
            return job;
        }


        private async void CompleteJob(BotAdapter adapter, string appId, ConversationReference conversation, int jobNumber)
        {
            await adapter.ContinueConversation(appId, conversation, async context =>
            {
                // Get the job log from state, and retrieve the job.
                var jobLog = GetJobLog(context);
                var job = jobLog[jobNumber];

                // Perform bookkeeping.
                job.Completed = true;

                // Send the user a proactive confirmation message.
                await context.SendActivity($"Job {job.JobNumber} is complete.");
            });
        }
    }  
}
