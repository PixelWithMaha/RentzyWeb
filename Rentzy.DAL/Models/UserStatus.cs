using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.DAL.Models
{
    public class UserStatus
    {
        public int Id { get; set; }

        // Foreign Key
        public int UserId { get; set; }
        public User User { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
