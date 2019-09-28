using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using ServiceDeskChatBot.Helpers;
using ServiceDeskChatBot.Models;
using ServiceDeskChatBot.Services;

namespace ServiceDeskChatBot.Dialogs
{
    public class GreetingDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        private readonly LuisService _luisService;

        #endregion  
        public GreetingDialog(string dialogId, BotStateService botStateService, LuisService luisService) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));
            _luisService = luisService ?? throw new System.ArgumentNullException(nameof(luisService)); ;

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                GetNamePartFromLuis,//try to get name immediately 
                InitialStepAsync,
                GetNamePartFromLuis,
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
            UserProfile userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            if (string.IsNullOrEmpty(userProfile.Name))
            {
                return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.name",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What is your name?")
                    }, cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> GetNamePartFromLuis(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            if (string.IsNullOrEmpty(userProfile.Name))
            {
                var result =
                    await _luisService.LuisRecognizer.RecognizeAsync(stepContext.Context, cancellationToken);

                var luisResult = result.Properties["luisResult"] as LuisResult;
                if (luisResult != null)
                {
                    var entitiesOfTypeBuiltinPersonName = luisResult.Entities.Where(p => p.Type.Equals(BotConstants.BuiltinPersonName)).ToList();
                    if (entitiesOfTypeBuiltinPersonName.Any())
                    {
                        foreach (var entity in entitiesOfTypeBuiltinPersonName)
                        {
                            stepContext.Values["Name"] = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(entity.Entity);
                                var setUserProfile = await WriteToUserProfile(stepContext);
                        }
                    }
                }
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await WriteToUserProfile(stepContext);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Hi {0}. How can I help you today?", userProfile.Name)), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<UserProfile> WriteToUserProfile(WaterfallStepContext stepContext)
        {
            UserProfile userProfile =
                await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                // Set the name
                userProfile.Name = (string) stepContext.Values["Name"];

                // Save any state changes that might have occured during the turn.
                await _botStateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            }

            return userProfile;
            //WriteToUserProfile userProfile =
            //    await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new WriteToUserProfile());
            //if (string.IsNullOrEmpty(userProfile.Name))
            //{
            //    // Set the name
            //    userProfile.Name = (string) stepContext.Result;

            //    // Save any state changes that might have occured during the turn.
            //    await _botStateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            //}

            //return userProfile;
        }
    }
}
