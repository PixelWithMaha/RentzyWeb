namespace Rentzy.DAL.Models
{
    public class NoUser : User
    {
        private static readonly NoUser _instance = new NoUser();

        public static NoUser Instance => _instance;

        private NoUser()
        {
            Id = 0;
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            PasswordHash = string.Empty;
            Phone = string.Empty;
            CreatedAt = DateTime.MinValue;
        }

    }
}