using System.Threading.Tasks;
using DebtCollectorBotWebApi.Data;

namespace DebtCollectorBotWebApi.Services
{
    internal interface IAccountingService
    {
        decimal AlCredit { get; set; }
        decimal Balance { get; set; }
        decimal BelCredit { get; set; }

        Task ChangeCreditByAsync(decimal amountToChangeBy, string spouseCode);
    }

    internal class AccountingService : IAccountingService
    {
        private readonly IMongoService _mongoService;

        public AccountingService(IMongoService mongoService)
        {
            _mongoService = mongoService;

            Balance = _mongoService.GetBalance();
            AlCredit = _mongoService.GetAlCredit();
            BelCredit = _mongoService.GetBelCredit();
        }

        public decimal Balance { get; set; }
        public decimal AlCredit { get; set; }
        public decimal BelCredit { get; set; }

        private async Task UpdateBalanceAsync()
        {
            Balance = AlCredit - BelCredit;
            await _mongoService.UpdateBalanceAsync(Balance);
        }

        public async Task ChangeCreditByAsync(decimal amountToChangeBy, string spouseCode)
        {
            if (spouseCode == "A") await ChangeAlCreditByAsync(amountToChangeBy);

            if (spouseCode == "B") await ChangeBelCreditByAsync(amountToChangeBy);
        }

        private async Task ChangeAlCreditByAsync(decimal amount)
        {
            AlCredit += amount;
            await _mongoService.UpdateAlCreditAsync(AlCredit);
            await UpdateBalanceAsync();
        }

        private async Task ChangeBelCreditByAsync(decimal amount)
        {
            BelCredit += amount;
            await _mongoService.UpdateBelCreditAsync(BelCredit);
            await UpdateBalanceAsync();
        }
    }
}