using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace DebtCollectorBotWebApi.Controllers
{
    [ApiController]
    public class BotController : ControllerBase
    {
        public BotController(ITelegramBotService telegramBotService)
        {
            _telegramBotService = telegramBotService;
        }

        private ITelegramBotService _telegramBotService { get; }

        [Route("/")]
        [HttpGet]
        public string Get()
        {
            return "Hello";
        }

        [Route("/{telegramBotService.ApiToken}")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            await _telegramBotService.HandleUpdateAsync(update);
            return Ok();
        }
    }
}