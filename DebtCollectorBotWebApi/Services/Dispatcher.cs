using DebtCollectorBotWebApi.Services;
using System;
using System.Threading.Tasks;

namespace DebtCollectorBotWebApi
{
    public interface IDispatcher
    {
        Task<ValidationResult> HandleTextCommandAsync(string message, string spouseCode);
        public decimal GetBalance();
    }

    internal class Dispatcher : IDispatcher
    {
        private IAccountingService _accountingService { get; }
        public Dispatcher(IAccountingService accountingService)
        {
            _accountingService = accountingService;
        }

        public async Task<ValidationResult> HandleTextCommandAsync(string message, string spouseCode)
        {
            var args = message.Split(' ');

            var validationResult = ValidateInput(args);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            await ProcessArgsAsync(args, spouseCode);

            return validationResult;
        }

        public decimal GetBalance()
        {
            return _accountingService.Balance;
        }

        private async Task ProcessArgsAsync(string[] args, string spouseCode)
        {
            var value = decimal.Parse(args[0].Replace(",", "."));

            if (args.Length == 1 && value == 0) return;

            if (args.Length == 1 && value != 0)
            {
                await _accountingService.ChangeCreditByAsync(value, spouseCode);
                return;
            }

            if (args.Length == 2)
            {
                await _accountingService.ChangeCreditByAsync(value / 2, spouseCode);
            }
        }

        private ValidationResult ValidateInput(string[] args)
        {
            var result = new ValidationResult();

            if (!decimal.TryParse(args[0], out var parsedAmount))
            {
                result.Success = false;
                var ErrorMessage = "first argument should be a number";
                result.ErrorMessages.Add(ErrorMessage);
            }

            if (args.Length >= 2 && args[1] != "о" && args[1] != "o")
            {
                result.Success = false;
                var ErrorMessage = "second argument should be an o";
                result.ErrorMessages.Add(ErrorMessage);
            }

            return result;
        }
    }
}