using Rentzy.DAL.Models;

namespace Rentzy.BLL.Factory
{
    public static class UserFactory
    {
        public static User CreateUser(string userType)
        {
            return userType.ToLower() switch
            {
                "tenant" => new Tenant(),
                "landlord" => new Landlord { IsVerified = false },
                "admin" => new Admin { Role = "Admin" },
                _ => throw new ArgumentException($"Invalid user type: {userType}")
            };
        }
    }
}