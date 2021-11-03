using System.Collections.Generic;

namespace DebtCollectorBotWebApi
{
    public class ValidationResult
    {
        public ValidationResult()
        {
            Success = true;
            ErrorMessages = new List<string>();
        }

        internal bool Success { get; set; }
        internal List<string> ErrorMessages { get; set; }
    }
}