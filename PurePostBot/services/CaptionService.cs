

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Services
{
    public class CaptionService(ITelegramBotClient bot, UserService userService, OptionsService optionsService)
    {
        private readonly ITelegramBotClient _bot = bot;
        private readonly UserService _userService = userService;
        private readonly OptionsService _optionsService = optionsService;

        public async Task<bool> HandleChangingCaptionAsync(Message message, long userId)
        {
            if ((await _userService.GetUserAsync(userId)).IsChangingCaption)
            {
                if (await TrySetCaptionAsync(message, userId))
                {
                    // Success message
                    await PostingService.Send(_bot, userId, new Message
                    { Text = RepliesReadService.GetReply("set_caption_success") });
                }
                else 
                {
                    // Failed message
                    await PostingService.Send(_bot, userId, new Message
                    { Text = RepliesReadService.GetReply("set_caption_failed") });
                }

                return true;
            }
            return false;
        }

        private async Task<bool> TrySetCaptionAsync(Message message, long userId)
        {
            // Get caption
            if (message.Text is { } text)
            {
                // Crop
                int limit = 1024;
                var caption = text.Length > limit ? text.Substring(0, limit) : text;

                await _optionsService.ChangeCaptionAsync(userId, caption); // Change user caption

                return true;
            }
            return false;

        }
    }
}
