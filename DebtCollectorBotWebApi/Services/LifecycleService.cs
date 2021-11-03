using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DebtCollectorBotWebApi.Services
{
    public class LifecycleService : IHostedService
    {
        private readonly ITelegramBotService _telegramBotService;

        public LifecycleService(ITelegramBotService telegramBotService)
        {
            _telegramBotService = telegramBotService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _telegramBotService.SetWebHookAsync(cancellationToken);
            await _telegramBotService.SendMessageToAl("включился");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            //await _telegramBotService.DeleteWebHookAsync(cancellationToken);
            Console.WriteLine("This is where the webhook would have been deleted");
            await _telegramBotService.SendMessageToAl("выключаюсь");
        }
    }
}