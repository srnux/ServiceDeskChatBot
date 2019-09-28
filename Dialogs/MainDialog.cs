using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using ServiceDeskChatBot.Services;

namespace ServiceDeskChatBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        private readonly LuisService _luisService;

        #endregion  


        public MainDialog(BotStateService botStateService, LuisService luisService) : base(nameof(MainDialog))
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
                InitialStepAsync,
                FinalStepAsync
            };

            // Add Named Dialogs
            AddDialog(new GreetingDialog($"{nameof(MainDialog)}.greeting", _botStateService, _luisService));
            AddDialog(new BugReportDialog($"{nameof(MainDialog)}.bugReport", _botStateService));
            AddDialog(new BugTypeDialog($"{nameof(MainDialog)}.bugType", _botStateService,_luisService));

            AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", waterfallSteps));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var recognizerResult =
                await _luisService.LuisRecognizer.RecognizeAsync(stepContext.Context, cancellationToken);

            var topIntent = recognizerResult.GetTopScoringIntent();

            switch (topIntent.intent)
            {
                case "GreetingIntent":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting", null,
                        cancellationToken);
                case "QueryBugTypeIntent":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugType", null,
                        cancellationToken);
                case "SubmitBugIntent":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugReport", null,
                        cancellationToken);
                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("I am sorry but I do not know what you mean."), cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
            //if (Regex.Match(stepContext.Context.Activity.Text.ToLower(), "hi").Success)
            //{
            //    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting", null, cancellationToken);
            //}
            //else
            //{
            //    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugReport", null, cancellationToken);
            //}
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
