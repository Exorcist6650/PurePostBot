using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PurePostBot.services
{
    public class PostEditService(ITelegramBotClient bot, UserService userService, PostingCacheService messagesCache)
    {
        private readonly ITelegramBotClient _bot = bot;
        private readonly UserService _userService = userService;
        private readonly PostingCacheService _messagesCache = messagesCache;
        private readonly InlineKeyboardMarkup _inlineKeyboard = new(
            [
                [
                    // CREATE POST button 
                    InlineKeyboardButton.WithCallbackData(
                        RepliesReadService.GetButton("editing_create_post"), "action:editing_send_post")
                ],
                [
                    // CAPTION button
                    InlineKeyboardButton.WithCallbackData(
                        RepliesReadService.GetButton("editing_caption"), "action:editing_add_caption")
                ],
                [
                    // CANCEL button
                    InlineKeyboardButton.WithCallbackData(
                        RepliesReadService.GetButton("cancel"), "action:editing_cancel")
                ]
            ]);


        // Method to construct and unconstruct tickets
        private static string CreateTicket(string key) => $"TICKET#{key}";

        private static string GetKey(string ticket) => ticket["TICKET#".Length..];


        public async Task CreateEditMessage(Message message)
        {
            if (message?.Chat.Id is not { } chatId) return;

            // Save message to chache
            var key = _messagesCache.Set(message);

            // Send edit buttons
            await SendButtons(chatId, key);
        }

        public async Task CreateEditMessage(ConcurrentQueue<Message> messages)
        {
            messages.TryPeek(out var first); // Get first
            if (first?.Chat.Id is not { } chatId) return;

            // Save messages to chache
            var key = _messagesCache.Set(messages);

            // Send edit buttons
            await SendButtons(chatId, key);
        }


        public async Task SendToGroup(CallbackQuery query)
        {
            if (query.Message is not { } message) return;
            if (message?.Chat.Id is not { } chatId) return;

            var key = GetKey(message.Text!); // Get key from message tiket


            // Read group id from db
            if ((await _userService.GetUserAsync(chatId)).GroupId is not { } groupId)
            {
                // Group id null message to user
                await PostingService.Send(_bot, chatId, new Message 
                    { Text = RepliesReadService.GetReply("group_id_null") });

                return;
            }

            // Get message like object. Otherwise alert user
            if (await TryGetFromChache(message, chatId, key) is not { } output) return;

            // Check admin right in group id
            if (!await GroupIdService.IsGroupMember(_bot, groupId))
            {
                // Haven't admin rights message to user
                await PostingService.Send(_bot, chatId, new Message
                    { Text = RepliesReadService.GetReply("not_admin_rights") });

                return;
            }


            // Case message
            if (output is Message outputMessage)
                await PostingService.Send(_bot, groupId, outputMessage);

            // Case album
            if (output is ConcurrentQueue<Message> outputMessages)
            {
                var outputMedia = MediaGroupService.ConvertToMediaGroup(outputMessages);
                await PostingService.SendAlbum(_bot, groupId, outputMedia);
            }

            // Success message
            await PostingService.Send(_bot, chatId, new Message
                { Text = RepliesReadService.GetReply("success_sending_group") });
        }

        public async Task AddCaption(CallbackQuery query)
        {
            if (query.Message is not { } message) return;
            if (message?.Chat.Id is not { } chatId) return;

            if ((await _userService.GetUserAsync(chatId)).Caption is not { } caption)
            {
                // Group id null message to user
                await PostingService.Send(_bot, chatId, new Message
                { Text = RepliesReadService.GetReply("user_caption_null") });

                return;
            }

            var key = GetKey(message.Text!); // Get key from message tiket

            // Get message like object. Otherwise alert user
            if (await TryGetFromChache(message, chatId, key) is not { } output) return;

            // Case message
            if (output is Message outputMessage)
            {
                outputMessage.Caption = caption; // Change caption
                _messagesCache.Set(outputMessage); // Update message in cache
            }

            // Case album
            if (output is ConcurrentQueue<Message> outputMessages)
            {
                outputMessages.TryPeek(out var firstOutputMessage); // Get first message

                firstOutputMessage!.Caption = caption; // Change caption

                _messagesCache.Set(outputMessages); // Update message in cache

            }

            // Success message
            await PostingService.Send(_bot, chatId, new Message
                { Text = RepliesReadService.GetReply("caption_added") });
        }

        public async Task CancelEditing(CallbackQuery query)
        {
            if (query.Message is not { } message) return;
            if (message?.Chat.Id is not { } chatId) return;

            var key = GetKey(message.Text!);

            _messagesCache.Remove(key); // Remove message from key

            await PostingService.Remove(_bot, chatId, message); // Delete editing message
        }

        private async Task SendButtons(ChatId chatId, string uniqueCode) =>
            await PostingService.Send(_bot, chatId, new Message 
                { Text = CreateTicket(uniqueCode), ReplyMarkup = _inlineKeyboard });

        private async Task<object?> TryGetFromChache(Message message, long chatId, string key)
        {
            if (_messagesCache.Get<object>(key) is not { } output)
            {
                // Unavailable ticket message to user
                await PostingService.Send(_bot, chatId, new Message
                { Text = RepliesReadService.GetReply("ticket_unavailable") });

                // Remove unavailable edit message cause it useless
                await PostingService.Remove(_bot, chatId, message);

                return null;
            }

            return output;
        }
    }
}
