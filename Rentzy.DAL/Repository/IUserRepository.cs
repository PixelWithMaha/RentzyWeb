using Rentzy.DAL.Models;
using System.Threading.Tasks;

namespace Rentzy.DAL.Repositories
{
    public interface IUserRepository
    {
        Task AddUser(User user);
        Task<User> GetUserByEmail(string username);
        Task<User> GetUserById(int id);
        Task UpdateUser(User user);
        Task DeleteUser(int id);
        Task SaveChanges();

        Task<List<User>> GetAllUsers();
        Task<List<Tenant>> GetAllTenants();
        Task<List<Landlord>> GetAllLandlords();
        Task<List<Landlord>> GetVerifiedLandlords();
        Task<List<User>> SearchUsers(string searchTerm);
    }
}