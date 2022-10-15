using AutorepairShop.Models;

namespace AutorepairShop.Services
{
    public interface ICachedOwnersService
    {
        public IEnumerable<Owner> GetOwners(int rowsNumber = 20);
        public void AddOwners(string cacheKey, int rowsNumber = 20);
        public IEnumerable<Owner> GetOwners(string cacheKey, int rowsNumber = 20);
    }
}
