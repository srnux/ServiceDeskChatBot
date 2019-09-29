using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using ServiceDeskChatBot.Helpers;
using ServiceDeskChatBot.Models.FacebookModels;
using ServiceDeskChatBot.Services;

namespace ServiceDeskChatBot.Dialogs
{
    public class BugTypeDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        private readonly LuisService _luisService;

        #endregion  
        public BugTypeDialog(string dialogId, BotStateService botStateService, LuisService luisService) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));
            _luisService = luisService ?? throw new System.ArgumentNullException(nameof(luisService)); 

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.name"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(GreetingDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result =
                await _luisService.LuisRecognizer.RecognizeAsync(stepContext.Context, cancellationToken);
            if (result.Properties["luisResult"] is LuisResult luisResult)
            {
                var entities = luisResult.Entities;

                foreach (var entity in entities)
                {
                    if (BotConstants.BugTypes.Any(p => p.Equals(entity.Entity, StringComparison.OrdinalIgnoreCase)))
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{entity.Entity} is a Bug Type!"),
                            cancellationToken);

                        await stepContext.Context.SendActivityAsync(ReplyFacebookMessage(stepContext.Context.Activity, entity.Entity), cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{entity.Entity} is NOT a Bug Type!"),
                            cancellationToken);
                    }
                }
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private static Activity ReplyFacebookMessage(Activity activity, string entityName)
        {
            
            var facebookMessage = new FacebookSendMessage
            {
                attachment = new FacebookAttachment()
                {
                    Type = FacebookAttachmentTypes.template, Payload = new FacebookPayload()
                    {
                        TemplateType = FacebookTemplateTypes.generic
                    }
                }
            };
            var bugType = new FacebookElement()
            {
                Title = entityName
            };

            switch (entityName.ToLower())
            {
                case "security":
                    bugType.ImageUrl = "https://c1.staticflickr.com/9/8604/16042227002_1d00e0771d_b.jpg";
                    bugType.Subtitle = "This is a description of the security bug type";
                    break;
                case "crash":
                    bugType.ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/5/50/Windows_7_BSOD.png";
                    bugType.Subtitle = "This is a description of the crash bug type";
                    break;
                case "power":
                    bugType.ImageUrl = "https://www.publicdomainpictures.net/en/view-image.php?image=1828&picture=power-button";
                    bugType.Subtitle = "This is a description of the power bug type";
                    break;
                case "performance":
                    bugType.ImageUrl =
                        "https://commons.wikimedia.org/wiki/File:High_Performance_Computing_Center_Stuttgart_HLRS_2015_07_Cray_XC40_Hazel_Hen_IO.jpg";
                    bugType.Subtitle = "This is a description of the performance bug type";
                    break;
                case "usability":
                    bugType.ImageUrl = "https://commons.wikimedia.org/wiki/File:03-Pau-DevCamp-usability-testing.jpg";
                    bugType.Subtitle = "This is a description of the usability bug type";
                    break;
                case "seriousbug":
                    bugType.ImageUrl = "https://commons.wikimedia.org/wiki/File:Computer_bug.svg";
                    bugType.Subtitle = "This is a description of the serious bug type";
                    break;
                case "other":
                    bugType.ImageUrl = "https://commons.wikimedia.org/wiki/File:Symbol_Resin_Code_7_OTHER.svg";
                    bugType.Subtitle = "This is a description of the other bug type";
                    break;
                default:
                    break;
            }

            facebookMessage.attachment.Payload.Elements = new FacebookElement[] {bugType};
            activity.ChannelData = facebookMessage;
            return activity;
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
