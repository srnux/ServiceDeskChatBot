using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;

namespace ServiceDeskChatBot.Services
{
    public class LuisService
    {
        public LuisService(IConfiguration configuration )
        {
            LuisRecognizer = new LuisRecognizer(
                new LuisApplication(configuration["LuisAppId"],
                    configuration["LuisAPIKey"],
                    $"https://{configuration["LuisAPIHostName"]}.api.cognitive.microsoft.com"),
                new LuisPredictionOptions {IncludeAllIntents = true, IncludeInstanceData = true},
                true);
        }

        public LuisRecognizer LuisRecognizer { get; private set; }
    }
}
