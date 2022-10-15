using AutorepairShop.Models;
using AutorepairShop.Data;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace AutorepairShop.Services
{
    public class CachedPaymentsService : ICachedPaymentsService
    {
        private readonly AutorepairContext _dbContext;
        private readonly IMemoryCache _memoryCache;

        public CachedPaymentsService(AutorepairContext dbContext, IMemoryCache memoryCache)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
        }

        public void AddPayments(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<Payment> payments = _dbContext.Payments.Take(rowsNumber).ToList();
            if (payments != null)
            {
                _memoryCache.Set(cacheKey, payments, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

            }
        }

        public IEnumerable<Payment> GetPayments(int rowsNumber = 20)
        {
            return _dbContext.Payments.Take(rowsNumber).ToList();
        }

        public IEnumerable<Payment> GetPayments(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<Payment> payments;
            if (!_memoryCache.TryGetValue(cacheKey, out payments))
            {
                payments = _dbContext.Payments.Take(rowsNumber).ToList();
                if (payments != null)
                {
                    _memoryCache.Set(cacheKey, payments,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
                }
            }
            return payments;
        }
    }
}
