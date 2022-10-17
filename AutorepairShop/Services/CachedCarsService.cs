using AutorepairShop.Models;
using AutorepairShop.Data;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace AutorepairShop.Services
{
    public class CachedCarsService : ICachedCarsService
    {
        private readonly AutorepairContext _dbContext;
        private readonly IMemoryCache _memoryCache;

        public CachedCarsService(AutorepairContext dbContext, IMemoryCache memoryCache)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
        }

        public void AddCars(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<Car> cars = _dbContext.Cars.Take(rowsNumber).ToList();
            if (cars != null)
            {
                _memoryCache.Set(cacheKey, cars, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(258)
                });
            }
        }

        public IEnumerable<Car> GetCars(int rowsNumber = 20)
        {
            return _dbContext.Cars.Take(rowsNumber).ToList();
        }

        public IEnumerable<Car> GetCars(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<Car> cars;
            if (!_memoryCache.TryGetValue(cacheKey, out cars))
            {
                cars = _dbContext.Cars.Take(rowsNumber).ToList();
                if (cars != null)
                {
                    _memoryCache.Set(cacheKey, cars,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(258)));
                }
            }
            return cars;
        }
    }
}
