using System;
using System.Text;
using System.Threading.Tasks;
using DebtCollectorBotWebApi.Services;

namespace DebtCollectorBotWebApi
{
    public interface IDispatcher
    {
        Task<string> GetResponseFromMessageAsync(string message, string spouseCode);
        public string GetBalanceMessage();
    }

    internal class Dispatcher : IDispatcher
    {
        public Dispatcher(IAccountingService accountingService)
        {
            _accountingService = accountingService;
        }

        private IAccountingService _accountingService { get; }

        public async Task<string> GetResponseFromMessageAsync(string message, string spouseCode)
        {
            var response = new StringBuilder();

            var args = message.Split(' ');

            var validationResult = ValidateInput(args);
            if (!validationResult.Success)
            {
                response.Clear();
                response.Append(string.Join(", ", validationResult.ErrorMessages));
                return response.ToString();
            }

            await ProcessArgsAsync(args, spouseCode);

            response.AppendLine(GetBalanceMessage());

            return response.ToString();
        }

        public string GetBalanceMessage()
        {
            var balance = _accountingService.Balance;

            if (balance > 0) return "белка дожна элу " + balance + " BYN";
            if (balance < 0) return "эл должен белке " + Math.Abs(balance) + " BYN";
            if (balance == 0) return "никто никому ничего не должен";
            return "weird balance message";
        }

        private async Task ProcessArgsAsync(string[] args, string spouseCode)
        {
            var value = decimal.Parse(args[0]);

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