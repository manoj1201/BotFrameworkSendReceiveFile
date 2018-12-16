using System;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            var messageToForward = await message as Activity;
            var content = await message;
            if (content.Attachments != null && content.Attachments.Any())
            {
                await context.Forward(new ReceiveAttachmentDialog(), this.ReceiveAttachment, messageToForward, CancellationToken.None);
            }
            else
            {
                await this.ShowLuisResult(context, result);
            }
        }

        [LuisIntent("GetFile")]
        public async Task GetApplicationFile(IDialogContext context, LuisResult result)
        {
            try
            {
                var replyMessage = context.MakeMessage();
                Attachment attachment = GetFileLink();
                replyMessage.Attachments = new List<Attachment> { attachment };
                await context.PostAsync(replyMessage);

            }
            catch (Exception ex)
            {

            }
        }

        private static Attachment GetFileLink()
        {
            return new Attachment
            {
                Name = "Visa Application Form",
                ContentType = "application/pdf",
                ContentUrl = "<SiteCollectionAbsolutePath>/BotFiles/VisaApplicationForm.pdf"
            };
        }


        private async Task ReceiveAttachment(IDialogContext context, IAwaitable<object> result)
        {
            context.Wait(this.MessageReceived);
        }


        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "Greeting" with the name of your newly created intent in the following handler
        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
            context.Wait(MessageReceived);
        }
    }
}