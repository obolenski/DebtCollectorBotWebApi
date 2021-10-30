namespace DebtCollectorBotWebApi.Data
{
    internal class DebtCollectorBotAccount
    {
        public int _id { get; set; }
        public decimal Balance { get; set; }
        public decimal AlCredit { get; set; }
        public decimal BelCredit { get; set; }
    }
}