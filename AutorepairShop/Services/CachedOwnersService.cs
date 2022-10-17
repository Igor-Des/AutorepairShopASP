using AutorepairShop.Models;
using AutorepairShop.Data;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace AutorepairShop.Services
{
    public class CachedOwnersService : ICachedOwnersService
    {
        private readonly AutorepairContext _dbContext;
        private readonly IMemoryCache _memoryCache;

        public CachedOwnersService(AutorepairContext dbContext, IMemoryCache memoryCache)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
        }

        // добавление списка владельцев авто в кэш
        public void AddOwners(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<Owner> owners = _dbContext.Owners.Take(rowsNumber).ToList();
            if (owners != null)
            {
                _memoryCache.Set(cacheKey, owners, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(258) // N = 9 var. Данные в кэше хранить неизменными в течение 2*N+240 секунд 
                });
            }
        }

        // получение списка владельцев авто из БД
        public IEnumerable<Owner> GetOwners(int rowsNumber = 20)
        {
            return _dbContext.Owners.Take(rowsNumber).ToList();
        }

        // получение списка владельцев из кэша или из базы, если нет в кэше
        public IEnumerable<Owner> GetOwners(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<Owner> owners;
            if (!_memoryCache.TryGetValue(cacheKey, out owners))
            {
                owners = _dbContext.Owners.Take(rowsNumber).ToList();
                if (owners != null)
                {
                    _memoryCache.Set(cacheKey, owners,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(258)));
                }
            }
            return owners;
        }
    }
}
