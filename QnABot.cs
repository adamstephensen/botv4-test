// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace AspNetCore_QnA_Bot
{
    public class QnABot : IBot
    {        
        public async Task OnTurn(ITurnContext context)
        {
            if (context.Activity.Type == ActivityTypes.Message && !context.Responded)
            {
                await context.SendActivity("No QnA Maker answers were found.");
            }
        }
    }    
}
