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
        private string ApiToken { get; }
        private long ObolenskiTGId { get; }
        private long WifeTGId { get; }
        private readonly ITelegramBotClient BotClient;
        private readonly IDispatcher _dispatcher;

        public TelegramBotService(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            ApiToken = Environment.GetEnvironmentVariable("TelegramBotApiToken");
            ObolenskiTGId = long.Parse(Environment.GetEnvironmentVariable("ObolenskiTGId"));
            WifeTGId = long.Parse(Environment.GetEnvironmentVariable("MyWifeTGId"));

            BotClient = new TelegramBotClient(ApiToken);
        }

        public async Task SetWebHookAsync(CancellationToken cancellationToken)
        {
            string webhookAddress = $"https://debtcollectorbot.herokuapp.com/{ApiToken}";
            Console.WriteLine("Webhook set at " + webhookAddress);
            await BotClient.SetWebhookAsync(
                url: webhookAddress,
                allowedUpdates: Array.Empty<UpdateType>(),
                cancellationToken: cancellationToken);
        }

        public async Task DeleteWebHookAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Webhook deleted");
            await BotClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
        }

        public async Task HandleUpdateAsync(Update update)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                await ReactToCallbackQuery(update);
            }

            if (update.Type == UpdateType.Message)
            {
                if (update.Message.Type == MessageType.Text)
                {
                    await AnswerTextMessage(update);
                }
            }
        }

        private async Task AnswerTextMessage(Update update)
        {
            User sender = update.Message.From;
            long chatId = update.Message.Chat.Id;

            Console.WriteLine($"Received a '{update.Message.Text}' message from " + sender.Id + ", " + sender.Username);

            long[] allowedIds = { ObolenskiTGId, WifeTGId };
            if (!allowedIds.Contains(sender.Id))
            {
                await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "vattela a pija 'nder culo");

                await SendMessageToAl(
                    $"чужие лезут. {update.Message.From.Id}, {update.Message.From.Username}: {update.Message.Text}"
                    );

                return;
            }

            string spouseCode = sender.Id == ObolenskiTGId ? "A" : "B";

            string response = await _dispatcher.GetResponseFromMessageAsync(update.Message.Text, spouseCode);

            string inlineButtonText = update.Message.From.Id == ObolenskiTGId
                                    ? "сказать Белке"
                                    : "сказать Элу";
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(inlineButtonText, "publish"),
                    }
                });

            await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: response,
                replyMarkup: inlineKeyboard
            );

            //if (spouseCode == "B")
            //{
            //    await SendMessageToAl($"Белка: {update.Message.Text}. {response}");
            //}
        }

        private async Task ReactToCallbackQuery(Update update)
        {
            if (update.CallbackQuery.Data == "publish")
            {
                string messageToSend = _dispatcher.GetBalanceMessage();

                if (update.CallbackQuery.From.Id == ObolenskiTGId)
                {
                    await SendMessageToWife(messageToSend);
                }

                if (update.CallbackQuery.From.Id == WifeTGId)
                {
                    await SendMessageToAl(messageToSend);
                }

                await BotClient.AnswerCallbackQueryAsync(
                    callbackQueryId : update.CallbackQuery.Id,
                    text: "я передал"
                    );
            }
        }

        public async Task SendMessageToAl(string v)
        {
            await BotClient.SendTextMessageAsync(
                chatId: ObolenskiTGId,
                text: v
            );
        }

        public async Task SendMessageToWife(string v)
        {
            await BotClient.SendTextMessageAsync(
                chatId: WifeTGId,
                text: v
            );
        }
    }
}
