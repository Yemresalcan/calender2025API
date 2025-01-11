using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using CalendarAPI.Models;

namespace CalendarAPI.Services
{
    public interface IMonthService
    {
        Task<List<Month>> GetAllAsync(string userId);
        Task<Month> GetByIdAsync(string id);
        Task<Month> CreateAsync(Month month);
        Task UpdateAsync(string id, Month month);
        Task DeleteAsync(string id);
    }

    public class MonthService : IMonthService
    {
        private readonly IMongoCollection<Month> _months;

        public MonthService(IMongoClient mongoClient, IConfiguration config)
        {
            var database = mongoClient.GetDatabase(config["MongoDbSettings:DatabaseName"]);
            _months = database.GetCollection<Month>("Months");
        }

        public async Task<List<Month>> GetAllAsync(string userId)
        {
            return await _months.Find(m => m.UserId == userId).ToListAsync();
        }

        public async Task<Month> GetByIdAsync(string id)
        {
            return await _months.Find(m => m.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Month> CreateAsync(Month month)
        {
            await _months.InsertOneAsync(month);
            return month;
        }

        public async Task UpdateAsync(string id, Month month)
        {
            await _months.ReplaceOneAsync(m => m.Id == id, month);
        }

        public async Task DeleteAsync(string id)
        {
            await _months.DeleteOneAsync(m => m.Id == id);
        }
    }
} 