using Microsoft.EntityFrameworkCore;
using Rentzy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rentzy.DAL.Models.LandlordApproval;

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
        public DbSet<PaymentNotification> PaymentNotifications { get; set; }

        // Reviews
        public DbSet<Review> Reviews { get; set; }
        public DbSet<LandlordApproval> LandlordApprovals { get; set; }
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
            //---------------------------------------------------------------------

            modelBuilder.Entity<LandlordApproval>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Relationship: Landlord → LandlordApproval (1-to-many)
                entity.HasOne(e => e.Landlord)
                    .WithMany()                       // ❗ No navigation property on Landlord
                    .HasForeignKey(e => e.LandlordId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relationship: ApprovalStatus → LandlordApproval (1-to-many)
                entity.HasOne(e => e.ApprovalStatus)
                    .WithMany()                       // ❗ No navigation collection needed
                    .HasForeignKey(e => e.ApprovalStatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.SubmittedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            //-----------------------------------------------------------------------
            modelBuilder.Entity<PropertyApprovalRequest>(entity =>
            {
                entity.ToTable("PropertyApprovalRequests");

                entity.HasKey(e => e.Id);

                // Property (One Approval Request -> One Property)
                entity.HasOne(e => e.property)
                      .WithMany(p => p.ApprovalRequests)
                      .HasForeignKey(e => e.PropertyId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Admin (Nullable)
                entity.HasOne(e => e.Admin)
                      .WithMany(a => a.ApprovalRequests)
                      .HasForeignKey(e => e.AdminId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Status
                entity.HasOne(e => e.Status)
                      .WithMany(s => s.ApprovalRequests)
                      .HasForeignKey(e => e.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Comments)
                      .HasMaxLength(2000);
            });

            //-----------------------------------------------------------------------
            base.OnModelCreating(modelBuilder);

        }
    }
}
