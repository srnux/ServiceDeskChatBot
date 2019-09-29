using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;

namespace ServiceDeskChatBot.Helpers
{
    public class AdapterWithInspection : BotFrameworkHttpAdapter
    {
        ///use - INSPECT attach 3a40e0e4-8fe1-4617-a855-364f7c0c5548 (session id)
        public AdapterWithInspection(IConfiguration configuration, InspectionState inspectionState, UserState userState, ConversationState conversationState)
            : base(configuration)
        {
            // Inspection needs credentials because it will be sending the Activities and User and Conversation State to the emulator
            var credentials = new MicrosoftAppCredentials(configuration["MicrosoftAppId"], configuration["MicrosoftAppPassword"]);

            Use(new InspectionMiddleware(inspectionState, userState, conversationState, credentials));
        }
    }
}
