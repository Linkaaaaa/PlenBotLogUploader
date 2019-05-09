﻿namespace PlenBotLogUploader.DiscordAPI
{
    public class DiscordAPIJSONWebhookResponse
    {
        // On success
        public string Name { get; set; }
        public string Channel_id { get; set; }
        public string Token { get; set; }
        public string Avatar { get; set; }
        public string Guild_id { get; set; }
        public string Id { get; set; }

        // On fail
        public int? Code { get; set; }
        public string Message { get; set; }

        public bool IsSuccess() => Code == null;
    }
}