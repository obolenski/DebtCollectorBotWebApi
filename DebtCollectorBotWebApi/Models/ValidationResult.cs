using System.Collections.Generic;

namespace DebtCollectorBotWebApi
{
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
