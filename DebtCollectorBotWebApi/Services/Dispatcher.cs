using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DebtCollectorBotWebApi
{
    public interface IDispatcher
    {
        Task<string> GetResponseFromMessageAsync(string message, string spouseCode);
    }

    internal class Dispatcher : IDispatcher
    {
        private IAccountingService _accountingService { get; set; }

        public Dispatcher(IAccountingService accountingService)
        {
            _accountingService = accountingService;
        }
        public async Task<string> GetResponseFromMessageAsync(string message, string spouseCode)
        {
            StringBuilder response = new StringBuilder();

            string[] args = message.Split(' ');

            ValidationResult validationResult = ValidateInput(args);
            if (!validationResult.Success)
            {
                response.Clear();
                response.Append(string.Join(", ", validationResult.ErrorMessages));
                return response.ToString();
            }

            await ProcessArgsAsync(args, spouseCode);

            decimal bal = _accountingService.Balance;

            response.AppendLine(GetBalanceMessage(bal));

            return response.ToString();
        }

        private async Task ProcessArgsAsync(string[] args, string spouseCode)
        {
            decimal value = decimal.Parse(args[0]);

            if (args.Length == 1 && value == 0)
            {
                return;
            }

            if (args.Length == 1 && value != 0)
            {
                await _accountingService.ChangeCreditByAsync(value, spouseCode);
                return;
            }

            if (args.Length == 2)
            {
                await _accountingService.ChangeCreditByAsync((value / 2), spouseCode);
                return;
            }
        }

        private string GetBalanceMessage(decimal balance)
        {
            if (balance > 0)
            {
                return "белка дожна элу " + balance + " BYN";
            }
            if (balance < 0)
            {
                return "эл должен белке " + Math.Abs(balance) + " BYN";
            }
            if (balance == 0)
            {
                return "никто никому ничего не должен";
            }
            return "weird balance message";
        }

        private ValidationResult ValidateInput(string[] args)
        {
            ValidationResult result = new ValidationResult();

            if (!decimal.TryParse(args[0], out decimal parsedAmount))
            {
                result.Success = false;
                string ErrorMessage = "first argument should be a number";
                result.ErrorMessages.Add(ErrorMessage);
            }

            if (args.Length >= 2 && args[1] != "о" && args[1] != "o")
            {
                result.Success = false;
                string ErrorMessage = "second argument should be an o";
                result.ErrorMessages.Add(ErrorMessage);
            }

            return result;
        }
    }

    internal class ValidationResult
    {
        internal bool Success { get; set; }
        internal List<string> ErrorMessages { get; set; }
        public ValidationResult()
        {
            Success = true;
            ErrorMessages = new List<string>();
        }
    }
}
