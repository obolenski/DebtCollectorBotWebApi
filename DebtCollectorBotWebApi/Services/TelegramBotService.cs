using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DebtCollectorBotWebApi
{
    public interface ITelegramBotService
    {
        Task HandleUpdateAsync(Update update);
        Task SetWebHookAsync(CancellationToken cancellationToken);
        Task DeleteWebHookAsync(CancellationToken cancellationToken);
    }

    internal class TelegramBotService : ITelegramBotService
    {
        private readonly IDispatcher _dispatcher;
        private readonly ITelegramBotClient _botClient;

        public TelegramBotService(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            ApiToken = Environment.GetEnvironmentVariable("TelegramBotApiToken");
            ObolenskiTgId = long.Parse(Environment.GetEnvironmentVariable("ObolenskiTGId"));
            WifeTgId = long.Parse(Environment.GetEnvironmentVariable("MyWifeTGId"));
            _botClient = new TelegramBotClient(ApiToken);
        }

        private string ApiToken { get; }
        private long ObolenskiTgId { get; }
        private long WifeTgId { get; }

        public async Task HandleUpdateAsync(Update update)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => HandleMessageUpdate(update),
                UpdateType.CallbackQuery => HandleCallbackQueryUpdate(update),
                _ => HandleUnknownUpdate(update)
            };

            await handler;
        }

        private async Task HandleMessageUpdate(Update update)
        {
            if (update.Message.Type != MessageType.Text) return;

            var sender = update.Message.From;
            var chatId = update.Message.Chat.Id;

            Console.WriteLine($"Received a '{update.Message.Text}' message from " + sender.Id + ", " + sender.Username);

            long[] allowedIds = { ObolenskiTgId, WifeTgId };
            if (!allowedIds.Contains(sender.Id))
            {
                await SendMessage("vattela a pija 'nder culo", chatId);
                return;
            }

            var spouseCode = sender.Id == ObolenskiTgId ? "A" : "B";
            var validationResult = await _dispatcher.HandleTextCommandAsync(update.Message.Text, spouseCode);
            if (!validationResult.Success)
            {
                var msg = string.Join(", ", validationResult.ErrorMessages);
                await SendMessage(msg, chatId);
                return;
            }

            await SendMessageWithInlineKeyboard(GetBalanceMessage(), chatId);
        }

        private static Task HandleUnknownUpdate(Update update)
        {
            Console.WriteLine($"Received an update but don't have a handler. {update}");
            return Task.CompletedTask;
        }

        private async Task HandleCallbackQueryUpdate(Update update)
        {
            var handler = update.CallbackQuery.Data switch
            {
                "publish" => HandlePublishCallbackQuery(update),
                "refresh_balance" => HandleRefreshCallbackQuery(update),
                _ => HandleUnknownUpdate(update)
            };
            await handler;
        }

        private async Task HandleRefreshCallbackQuery(Update update)
        {
            var chatId = update.CallbackQuery.From.Id;
            await SendMessageWithInlineKeyboard(GetBalanceMessage(), chatId);
            await _botClient.AnswerCallbackQueryAsync(
                update.CallbackQuery.Id,
                "свежий баланс"
            );
        }

        private async Task HandlePublishCallbackQuery(Update update)
        {
            var messageToSend = GetBalanceMessage();

            if (update.CallbackQuery.From.Id == ObolenskiTgId) await SendMessage(messageToSend, WifeTgId);
            if (update.CallbackQuery.From.Id == WifeTgId) await SendMessage(messageToSend, ObolenskiTgId);

            await _botClient.AnswerCallbackQueryAsync(
                update.CallbackQuery.Id,
                "я передал"
            );
        }

        private async Task SendMessage(string msg, long chatId)
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                msg
            );
        }

        private async Task SendMessageWithInlineKeyboard(string msg, long chatId)
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                msg,
                replyMarkup: GetInlineKeyboard(chatId)
            );
        }

        private InlineKeyboardMarkup GetInlineKeyboard(long chatId)
        {
            var publishButtonText = chatId == ObolenskiTgId
                            ? "сказать белке"
                            : "сказать элу";
            var refreshBalanceButtonText = "узнать баланс";
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(publishButtonText, "publish"),
                    InlineKeyboardButton.WithCallbackData(refreshBalanceButtonText, "refresh_balance")
                }
            });
            return inlineKeyboard;
        }

        private string GetBalanceMessage()
        {
            var balance = _dispatcher.GetBalance();

            switch (balance)
            {
                case > 0:
                    return "белка дожна элу " + balance + " BYN";
                case < 0:
                    return "эл должен белке " + Math.Abs(balance) + " BYN";
                case 0:
                    return "никто никому ничего не должен";
            }
        }

        public async Task SetWebHookAsync(CancellationToken cancellationToken)
        {
            var webhookAddress = $"https://debtcollectorbot.herokuapp.com/{ApiToken}";
            Console.WriteLine("Webhook set at " + webhookAddress);
            await _botClient.SetWebhookAsync(
                webhookAddress,
                allowedUpdates: Array.Empty<UpdateType>(),
                cancellationToken: cancellationToken);
        }

        public async Task DeleteWebHookAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Webhook deleted");
            await _botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
        }
    }
}