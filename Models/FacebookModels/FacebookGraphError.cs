namespace ServiceDeskChatBot.Models.FacebookModels
{
    public class FacebookGraphError
    {
        public string message { get; set; }
        public string type { get; set; }
        public int code { get; set; }
        public int error_subcode { get; set; }
        public string fbtrace_id { get; set; }
    }
}
