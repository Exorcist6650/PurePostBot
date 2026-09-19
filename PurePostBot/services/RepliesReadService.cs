using System.Text.Json;

namespace PurePostBot.services
{
    public enum ELangs : byte
    {
        En,
        Ru,
    }
    public static class RepliesReadService
    {
        private const string PATH_TO_REPLIES = "resources\\BotReplies.json";
        private const string PATH_TO_BUTTONS = "resources\\BotButtons.json";

        private static readonly string _replies = File.ReadAllText(PATH_TO_REPLIES);
        private static readonly string _buttons = File.ReadAllText(PATH_TO_BUTTONS);

        public static string GetReply(string name, ELangs lang = ELangs.En)
        {
            // Parsing text to json
            var jsonDocument = JsonDocument.Parse(_replies);
            var root = jsonDocument.RootElement;

            // Get language group
            switch(lang)
            {
                case ELangs.En:
                    if (!root.TryGetProperty("en", out root)) throw new NullReferenceException("English reply not found");
                    break;

                case ELangs.Ru:
                    if (!root.TryGetProperty("ru", out root)) throw new NullReferenceException("Russian reply not found");
                    break;
            }

            // Get bot reply text
            if (!root.TryGetProperty(name, out var reply)) throw new NullReferenceException("Reply name is incorrect");

            return reply.ToString();
        }

        public static string GetButton(string name, ELangs lang = ELangs.En)
        {
            // Parsing text to json
            var jsonDocument = JsonDocument.Parse(_buttons);
            var root = jsonDocument.RootElement;

            // Get language group
            switch (lang)
            {
                case ELangs.En:
                    if (!root.TryGetProperty("en", out root)) throw new NullReferenceException("English button not found");
                    break;
                case ELangs.Ru:
                    if (!root.TryGetProperty("ru", out root)) throw new NullReferenceException("Russian button not found");
                    break;
            }

            // Get bot reply text
            if (!root.TryGetProperty(name, out var reply)) throw new NullReferenceException("Reply name is incorrect");

            return reply.ToString();
        }
    }
}
