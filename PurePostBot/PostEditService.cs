using System.Collections.Concurrent;
using DataManagement;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Services
{
    public class PostEditService
    {
        private readonly ITelegramBotClient _bot;
        private readonly UserService _userService;
        private readonly PostingMessagesCache _messagesCache;
        private readonly InlineKeyboardMarkup _inlineKeyboard;

        public PostEditService(ITelegramBotClient bot, UserService userService, PostingMessagesCache messagesCache)
        {
            _bot = bot;
            _userService = userService;
            _messagesCache = messagesCache;

            // Buttons
            _inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    // CREATE POST button 
                    InlineKeyboardButton.WithCallbackData(
                        RepliesReadService.GetButton("editing_create_post"), "action:editing_send_post")
                },
                new[]
                {
                    // CANCEL button
                    InlineKeyboardButton.WithCallbackData(
                        RepliesReadService.GetButton("cancel"), "action:editing_cancel")
                }
            });
        }

        public string CreateTicket(string key) => $"TICKET:{key}";

        public string GetKey(string ticket) => ticket.Substring("TICKET:".Length);

        public async Task CreateEditMessage(Message message)
        {
            if (message?.Chat.Id is not { } chatId) return;

            // Save message to chache
            var key = _messagesCache.Append(message);

            // Send edit buttons
            await SendButtons(chatId, key);
        }

        public async Task CreateEditMessage(ConcurrentQueue<Message> messages)
        {
            messages.TryPeek(out var first); // Get first
            if (first?.Chat.Id is not { } chatId) return;

            // Save messages to chache
            var key = _messagesCache.Append(messages);

            // Send edit buttons
            await SendButtons(chatId, key);
        }

        public async Task CancelEditing(CallbackQuery query)
        {
            if (query.Message is not { } message) return;
            if (message?.Chat.Id is not { } chatId) return;

            var key = GetKey(message.Text!);

            _messagesCache.Remove(key); // Remove message from key

            await PostingService.Remove(_bot, chatId, message); // Delete editing message
        }

        public async Task SendToGroup(CallbackQuery query)
        {
            if (query.Message is not { } message) return;
            if (message?.Chat.Id is not { } chatId) return;

            var key = GetKey(message.Text!); // Get key to find media in cache


            // Read group id from db
            if ((await _userService.GetUserAsync(chatId)).GroupId is not { } groupId)
            {
                // Group id null message to user
                await PostingService.Send(_bot, chatId, new Message 
                    { Text = RepliesReadService.GetReply("group_id_null") });

                return;
            }
            
            // Get object from cache
            if (_messagesCache.Get<object>(key) is not { } output)
            {
                // Unavailable ticket message to user
                await PostingService.Send(_bot, chatId, new Message
                    { Text = RepliesReadService.GetReply("ticket_unavailable") });

                return;
            }

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

        private async Task SendButtons(ChatId chatId, string uniqueCode) =>
            await PostingService.Send(_bot, chatId, new Message 
                { Text = CreateTicket(uniqueCode), ReplyMarkup = _inlineKeyboard });
    }
}
