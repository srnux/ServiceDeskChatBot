using System.Collections.Generic;

namespace ServiceDeskChatBot.Helpers
{
    public static class BotConstants
    {
        public static readonly List<string> BugTypes = new List<string> { "Security", "Crash", "Power","Performance", "Bug", "Usability","Other"};
        public const string BuiltinPersonName = "builtin.personName";
    }

}
