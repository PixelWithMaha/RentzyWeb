using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentzy.DAL.Models
{
    public class Admin : User
    {
        public string Role { get; set; } = "Admin";
    }
}
