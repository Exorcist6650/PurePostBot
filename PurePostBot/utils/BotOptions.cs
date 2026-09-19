namespace PurePostBot.utils
{
    public sealed class BotOptions
    {
        public const string SectionName = "Bot";
        public required string Token { get; set; } = string.Empty;
    }
}
