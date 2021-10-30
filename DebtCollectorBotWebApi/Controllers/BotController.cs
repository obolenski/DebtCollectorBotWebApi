using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DebtCollectorBotWebApi.Controllers
{
    [ApiController]
    public class BotController : ControllerBase
    {
        private ITelegramBotService _telegramBotService { get; set; }
        public BotController(ITelegramBotService telegramBotService)
        {
            _telegramBotService = telegramBotService;
        }

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
