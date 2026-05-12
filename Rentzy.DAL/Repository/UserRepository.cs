using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using Rentzy.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly RentzyDBContext _context;

    public UserRepository(RentzyDBContext context)
    {
        _context = context;
    }

    public async Task AddUser(User user)
    {
        if (user == null || user is NoUser)
            throw new ArgumentException("Cannot add null or NoUser");

        await _context.Users.AddAsync(user);
    }

    public async Task<User> GetUserByEmail(string email)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        return user ?? NoUser.Instance;
    }

    public async Task<User> GetUserById(int id)
    {
        var user = await _context.Users.FindAsync(id);
        return user ?? NoUser.Instance;
    }

    public Task UpdateUser(User user)
    {
        if (user is not NoUser)
        {
            _context.Users.Update(user);
        }
        return Task.CompletedTask;
    }

    public async Task DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
    }

    public async Task SaveChanges()
    {
        await _context.SaveChangesAsync();
    }

    // Get all users
    public async Task<List<User>> GetAllUsers()
    {
        return await _context.Users.ToListAsync();
    }

    // Get users by type
    public async Task<List<Tenant>> GetAllTenants()
    {
        return await _context.Users
            .OfType<Tenant>()
            .ToListAsync();
    }

    public async Task<List<Landlord>> GetAllLandlords()
    {
        return await _context.Users
            .OfType<Landlord>()
            .ToListAsync();
    }

    // Get verified landlords only
    public async Task<List<Landlord>> GetVerifiedLandlords()
    {
        return await _context.Users
            .OfType<Landlord>()
            .Where(l => l.IsVerified)
            .ToListAsync();
    }

    // Search users
    public async Task<List<User>> SearchUsers(string searchTerm)
    {
        return await _context.Users
            .Where(u => u.FirstName.Contains(searchTerm) ||
                        u.LastName.Contains(searchTerm) ||
                        u.Email.Contains(searchTerm))
            .ToListAsync();
    }
}