using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using SupportDeskBot.Helpers;
using SupportDeskBot.Services;

namespace SupportDeskBot.Dialogs
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
            var luisResult = result.Properties["luisResult"] as LuisResult;
            if (luisResult != null)
            {
                var entities = luisResult.Entities;

                foreach (var entity in entities)
                {
                    if (BotConstants.BugTypes.Any(p=>p.Equals(entity.Entity, StringComparison.OrdinalIgnoreCase)))
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{entity.Entity} is a Bug Type!"),
                            cancellationToken);
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

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
