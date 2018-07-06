// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Ai.QnA;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Azure;
namespace AspNetCore_QnA_Bot
{
    public class Startup
    {

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddBot<QnABot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);

                //QNA middleware configuration 
                //Each time new activity is recived it goes via middleware
                var qnaEndpoint = GetQnAMakerEndpoint(Configuration);
                var qnaMiddleware = new QnAMakerMiddleware(qnaEndpoint);
                options.Middleware.Add(qnaMiddleware);



       /*         // Storing State in the form of State form
                options.Middleware.Add(new ConversationState<State>(
                            new AzureTableStorage("DefaultEndpointsProtocol=https;AccountName=qutdemodotnetv4a6a4;AccountKey=2RHTNZprcLsjniApNy" +
                            "Fttb5MTu72EkE995SzIyKpUkhjW8g8RvogaCl2flzC9k4RuNEyDfn+J0Qtz4zOmCFBfg==", "conversationstatetable")));
                options.EnableProactiveMessages = true;*/


                // Exeption middleware lauer
                options.Middleware.Add(new CatchExceptionMiddleware<Exception>(async (context, exception) =>
                {
                    await context.TraceActivity("EchoBot Exception", exception);
                    await context.SendActivity("Sorry, it looks like something went wrong!");
                }));



                // Another time of storage in Memory for job execution 
                IStorage dataStore = new MemoryStorage();
                options.Middleware.Add(
                    new BotState<Dictionary<int, JobData>>(
                        dataStore, JobData.PropertyName, (context) => $"jobs/{typeof(Dictionary<int, JobData>)}"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }


        private QnAMakerEndpoint GetQnAMakerEndpoint(IConfiguration configuration)
        {
            var host = configuration.GetSection("QnAMaker-Host")?.Value;
            var knowledgeBaseId = configuration.GetSection("QnAMaker-KnowledgeBaseId")?.Value;
            var endpointKey = configuration.GetSection("QnAMaker-EndpointKey")?.Value;
            return new QnAMakerEndpoint
            {
                Host = host,
                KnowledgeBaseId = knowledgeBaseId,
                EndpointKey = endpointKey
            };
        }
    }
}
