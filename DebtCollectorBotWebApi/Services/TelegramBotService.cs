using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
        private long MyWifeTGId { get; }
        private readonly ITelegramBotClient BotClient;
        private readonly IDispatcher _dispatcher;

        public TelegramBotService(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            ApiToken = Environment.GetEnvironmentVariable("TelegramBotApiToken");
            ObolenskiTGId = long.Parse(Environment.GetEnvironmentVariable("ObolenskiTGId"));
            MyWifeTGId = long.Parse(Environment.GetEnvironmentVariable("MyWifeTGId"));

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
            if (update.Type != UpdateType.Message)
            {
                return;
            }

            if (update.Message.Type != MessageType.Text)
            {
                return;
            }

            User sender = update.Message.From;
            long chatId = update.Message.Chat.Id;

            Console.WriteLine($"Received a '{update.Message.Text}' message from " + sender.Id + ", " + sender.Username);

            long[] allowedIds = { ObolenskiTGId, MyWifeTGId };
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

            await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: response
            );

            if (spouseCode == "B")
            {
                await SendMessageToAl($"Белка: {update.Message.Text}. {response}");
            }
        }

        public async Task SendMessageToAl(string v)
        {
            await BotClient.SendTextMessageAsync(
                chatId: ObolenskiTGId,
                text: v
            );
        }
    }
}
