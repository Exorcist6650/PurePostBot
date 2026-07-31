using System.Text.Json;

namespace Services
{
    public enum ELangs : byte
    {
        En,
        Ru,
    }
    public static class RepliesReadService
    {
        private const string PATH_TO_JSON = "resources\\BotReplies.json";
        private static readonly string _text = File.ReadAllText(PATH_TO_JSON);

        public static string GetReply(string name, ELangs lang = ELangs.En)
        {
            // Parsing text to json
            var jsonDocument = JsonDocument.Parse(_text);
            var root = jsonDocument.RootElement;

            // Get language group
            switch(lang)
            {
                case ELangs.En:
                    if (!root.TryGetProperty("en", out root)) throw new NullReferenceException("English replies not found");
                    break;

                case ELangs.Ru:
                    if (!root.TryGetProperty("ru", out root)) throw new NullReferenceException("Russian replies not found");
                    break;
            }

            // Get bot reply text
            if (!root.TryGetProperty(name, out var reply)) throw new NullReferenceException("Reply name is incorrect");

            return reply.ToString();
        }
    }
}
