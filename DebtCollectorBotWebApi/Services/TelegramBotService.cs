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
        Task SendMessageToAl(string v);
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

        public async Task SendMessageToAl(string v)
        {
            await _botClient.SendTextMessageAsync(
                ObolenskiTgId,
                v
            );
        }

        private static Task HandleUnknownUpdate(Update update)
        {
            Console.WriteLine($"Received an update but don't have a handler. {update}");
            return Task.CompletedTask;
        }

        private async Task HandleMessageUpdate(Update update)
        {
            var sender = update.Message.From;
            var chatId = update.Message.Chat.Id;

            Console.WriteLine($"Received a '{update.Message.Text}' message from " + sender.Id + ", " + sender.Username);

            long[] allowedIds = {ObolenskiTgId, WifeTgId};
            if (!allowedIds.Contains(sender.Id))
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "vattela a pija 'nder culo");

                await SendMessageToAl(
                    $"чужие лезут. {update.Message.From.Id}, {update.Message.From.Username}: {update.Message.Text}"
                );

                return;
            }

            var spouseCode = sender.Id == ObolenskiTgId ? "A" : "B";

            var response = await _dispatcher.GetResponseFromMessageAsync(update.Message.Text, spouseCode);

            var inlineButtonText = update.Message.From.Id == ObolenskiTgId
                ? "сказать Белке"
                : "сказать Элу";
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(inlineButtonText, "publish")
                }
            });

            await _botClient.SendTextMessageAsync(
                chatId,
                response,
                replyMarkup: inlineKeyboard
            );

            //if (spouseCode == "B")
            //{
            //    await SendMessageToAl($"Белка: {update.Message.Text}. {response}");
            //}
        }

        private async Task HandleCallbackQueryUpdate(Update update)
        {
            if (update.CallbackQuery.Data == "publish")
            {
                var messageToSend = _dispatcher.GetBalanceMessage();

                if (update.CallbackQuery.From.Id == ObolenskiTgId) await SendMessageToWife(messageToSend);

                if (update.CallbackQuery.From.Id == WifeTgId) await SendMessageToAl(messageToSend);

                await _botClient.AnswerCallbackQueryAsync(
                    update.CallbackQuery.Id,
                    "я передал"
                );
            }
        }

        public async Task SendMessageToWife(string v)
        {
            await _botClient.SendTextMessageAsync(
                WifeTgId,
                v
            );
        }
    }
}