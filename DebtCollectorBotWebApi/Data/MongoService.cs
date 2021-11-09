using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace DebtCollectorBotWebApi.Data
{
    public interface IMongoService
    {
        decimal GetAlCredit();
        decimal GetBalance();
        decimal GetBelCredit();
        Task UpdateAlCreditAsync(decimal amount);
        Task UpdateBalanceAsync(decimal newBalance);
        Task UpdateBelCreditAsync(decimal amount);
    }

    public class MongoService : IMongoService
    {
        public MongoService()
        {
            var mongoPass = Environment.GetEnvironmentVariable("MongoPass");
            var connectionString =
                $"mongodb+srv://obolenski:{mongoPass}@debtcollectorbot.mdmzo.mongodb.net/DebtCollectorDb?retryWrites=true&w=majority";
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            var client = new MongoClient(settings);
            var database = client.GetDatabase("DebtCollectorDb");

            Collection = database.GetCollection<DebtCollectorBotAccount>("DebtCollectorBotAccounts");
            Filter = Builders<DebtCollectorBotAccount>.Filter.Eq("_id", 1);
            Account = Collection.Find(Filter).FirstOrDefault();
        }

        private IMongoCollection<DebtCollectorBotAccount> Collection { get; }
        private FilterDefinition<DebtCollectorBotAccount> Filter { get; }
        private DebtCollectorBotAccount Account { get; }

        public decimal GetAlCredit()
        {
            return Account.AlCredit;
        }

        public decimal GetBalance()
        {
            return Account.Balance;
        }

        public decimal GetBelCredit()
        {
            return Account.BelCredit;
        }

        public async Task UpdateAlCreditAsync(decimal amount)
        {
            var update = Builders<DebtCollectorBotAccount>.Update.Set("AlCredit", amount);
            await UpdateAccountAsync(update);
        }

        public async Task UpdateBalanceAsync(decimal newBalance)
        {
            var update = Builders<DebtCollectorBotAccount>.Update.Set("Balance", newBalance);
            await UpdateAccountAsync(update);
        }

        public async Task UpdateBelCreditAsync(decimal amount)
        {
            var update = Builders<DebtCollectorBotAccount>.Update.Set("BelCredit", amount);
            await UpdateAccountAsync(update);
        }

        private async Task UpdateAccountAsync(UpdateDefinition<DebtCollectorBotAccount> update)
        {
            await Collection.UpdateOneAsync(Filter, update);
        }
    }
}