using Rentzy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Rentzy.DAL.Context
{
    public class RentzyDBContext : DbContext
    {
        public RentzyDBContext(DbContextOptions<RentzyDBContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Landlord> Landlords { get; set; }
    }
}
