using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using SP = Microsoft.SharePoint.Client;

namespace Microsoft.Bot.Sample.LuisBot
{
    [Serializable]
    public class ReceiveAttachmentDialog : IDialog
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            if (message.Attachments != null && message.Attachments.Any())
            {
                var attachment = message.Attachments.First();
                var attachmentUrl = message.Attachments[0].ContentUrl;
                var content = message.Attachments[0].Content;

                using (HttpClient httpClient = new HttpClient())
                {
                    try
                    {
                        var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);
                        var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;
                        var attachmentdata = await httpClient.GetByteArrayAsync(attachmentUrl);

                        string siteUrl = Convert.ToString(ConfigurationManager.AppSettings["SiteUrl"]);
                        string login = Convert.ToString(ConfigurationManager.AppSettings["ApplicationUserName"]);
                        string password = Convert.ToString(ConfigurationManager.AppSettings["Password"]);
                        string listName = Convert.ToString(ConfigurationManager.AppSettings["DocumentLib"]);
                        var securePassword = new SecureString();
                        foreach (var c in password)
                        {
                            securePassword.AppendChar(c);
                        }
                        var credentials = new SP.SharePointOnlineCredentials(login, securePassword);
                        SP.ClientContext clientContext = new SP.ClientContext(siteUrl);
                        clientContext.Credentials = credentials;
                        SP.List documentsList = clientContext.Web.Lists.GetByTitle(listName);
                        var fileCreationInformation = new SP.FileCreationInformation();

                        //Assign to content byte[] i.e. documentStream  
                        fileCreationInformation.ContentStream = new MemoryStream(attachmentdata);

                        //Allow owerwrite of document
                        fileCreationInformation.Overwrite = true;

                        //Upload URL
                        fileCreationInformation.Url = siteUrl + "/" + listName + "/" + attachment.Name;
                        SP.File uploadFile = documentsList.RootFolder.Files.Add(
                            fileCreationInformation);

                        uploadFile.ListItemAllFields.Update();

                        clientContext.ExecuteQuery();
                        SP.ListItem item = uploadFile.ListItemAllFields;
                        string filenameWithoutExtension = Path.GetFileNameWithoutExtension(attachment.Name);
                        item["Title"] = filenameWithoutExtension;
                        item.Update();
                        clientContext.Load(item);
                        clientContext.ExecuteQuery();
                        //of {attachment.ContentType} type and size of {contentLenghtBytes} bytes received
                        await context.PostAsync($"Thanks for submitting the attachement.");
                    }
                    catch (Exception ex)
                    {

                    }
                }



            }
            else
            {
                await context.PostAsync("Hi there! I'm a bot created to show you how I can receive message attachments, but no attachment was sent to me. Please, try again sending a new message including an attachment.");
            }

            context.Wait(this.MessageReceivedAsync);
        }


    }
}