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

        // Properties & related tables
        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyType> PropertyTypes { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }

        // Bookings & Rental Requests
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingStatus> BookingStatuses { get; set; }
        public DbSet<PropertyRentalRequest> PropertyRentalRequests { get; set; }
        public DbSet<ApprovalStatus> RentalRequestStatuses { get; set; }

        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<PaymentStatus> PaymentStatuses { get; set; }

        // Approvals
        public DbSet<PropertyApprovalRequest> PropertyApprovalRequests { get; set; }
        public DbSet<ApprovalStatus> ApprovalStatuses { get; set; }

        // Reviews
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ignore NoUser (Null Object Pattern)
            modelBuilder.Ignore<NoUser>();

            // Configure decimal precision for Payment.Amount
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            // Set default value for User.CreatedAt
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            base.OnModelCreating(modelBuilder);
        }
    }
}
