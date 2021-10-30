using MongoDB.Driver;
using System;
using System.Linq;
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
        private IMongoCollection<DebtCollectorBotAccount> Collection { get; set; }
        private FilterDefinition<DebtCollectorBotAccount> Filter { get; set; }
        private DebtCollectorBotAccount Account { get; set; }
        public MongoService()
        {
            string mongoPass = Environment.GetEnvironmentVariable("MongoPass");
            string connectionString = $"mongodb+srv://obolenski:{mongoPass}@debtcollectorbot.mdmzo.mongodb.net/DebtCollectorDb?retryWrites=true&w=majority";
            MongoClientSettings settings = MongoClientSettings.FromConnectionString(connectionString);
            MongoClient client = new MongoClient(settings);
            IMongoDatabase database = client.GetDatabase("DebtCollectorDb");

            Collection = database.GetCollection<DebtCollectorBotAccount>("DebtCollectorBotAccounts");
            Filter = Builders<DebtCollectorBotAccount>.Filter.Eq("_id", 1);
            Account = Collection.Find(Filter).FirstOrDefault();
        }

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
            UpdateDefinition<DebtCollectorBotAccount> update = Builders<DebtCollectorBotAccount>.Update.Set("AlCredit", amount);
            await UpdateAccountAsync(update);
        }

        public async Task UpdateBalanceAsync(decimal newBalance)
        {
            UpdateDefinition<DebtCollectorBotAccount> update = Builders<DebtCollectorBotAccount>.Update.Set("Balance", newBalance);
            await UpdateAccountAsync(update);
        }

        public async Task UpdateBelCreditAsync(decimal amount)
        {
            UpdateDefinition<DebtCollectorBotAccount> update = Builders<DebtCollectorBotAccount>.Update.Set("BelCredit", amount);
            await UpdateAccountAsync(update);
        }

        private async Task UpdateAccountAsync(UpdateDefinition<DebtCollectorBotAccount> update)
        {
            await Collection.UpdateOneAsync(Filter, update);
        }
    }
}
