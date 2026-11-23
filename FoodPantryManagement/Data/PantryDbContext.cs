using FoodPantryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodPantryManagement.Data;

public sealed class PantryDbContext(DbContextOptions<PantryDbContext> options) : DbContext(options)
{
    public DbSet<PantrySite> PantrySites => Set<PantrySite>();
    public DbSet<FoodCategory> FoodCategories => Set<FoodCategory>();
    public DbSet<FoodItem> FoodItems => Set<FoodItem>();
    public DbSet<Donor> Donors => Set<Donor>();
    public DbSet<Household> Households => Set<Household>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<InventoryLot> InventoryLots => Set<InventoryLot>();
    public DbSet<Distribution> Distributions => Set<Distribution>();
    public DbSet<DistributionLine> DistributionLines => Set<DistributionLine>();
    public DbSet<Volunteer> Volunteers => Set<Volunteer>();
    public DbSet<VolunteerShift> VolunteerShifts => Set<VolunteerShift>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FoodCategory>().HasIndex(category => category.Name).IsUnique();
        modelBuilder.Entity<FoodItem>()
            .HasOne(item => item.FoodCategory)
            .WithMany(category => category.FoodItems)
            .HasForeignKey(item => item.FoodCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Donation>()
            .HasOne(donation => donation.Donor)
            .WithMany(donor => donor.Donations)
            .HasForeignKey(donation => donation.DonorId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Donation>()
            .HasOne(donation => donation.PantrySite)
            .WithMany(site => site.Donations)
            .HasForeignKey(donation => donation.PantrySiteId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryLot>()
            .HasOne(lot => lot.Donation)
            .WithMany(donation => donation.Lots)
            .HasForeignKey(lot => lot.DonationId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryLot>()
            .HasOne(lot => lot.PantrySite)
            .WithMany(site => site.InventoryLots)
            .HasForeignKey(lot => lot.PantrySiteId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryLot>()
            .HasOne(lot => lot.FoodItem)
            .WithMany(item => item.InventoryLots)
            .HasForeignKey(lot => lot.FoodItemId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Distribution>()
            .HasOne(distribution => distribution.Household)
            .WithMany(household => household.Distributions)
            .HasForeignKey(distribution => distribution.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Distribution>()
            .HasOne(distribution => distribution.PantrySite)
            .WithMany(site => site.Distributions)
            .HasForeignKey(distribution => distribution.PantrySiteId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<DistributionLine>()
            .HasOne(line => line.Distribution)
            .WithMany(distribution => distribution.Lines)
            .HasForeignKey(line => line.DistributionId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<DistributionLine>()
            .HasOne(line => line.FoodItem)
            .WithMany(item => item.DistributionLines)
            .HasForeignKey(line => line.FoodItemId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<DistributionLine>()
            .HasOne(line => line.InventoryLot)
            .WithMany()
            .HasForeignKey(line => line.InventoryLotId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Volunteer>()
            .HasIndex(volunteer => volunteer.Email)
            .IsUnique();
        modelBuilder.Entity<VolunteerShift>()
            .HasOne(shift => shift.Volunteer)
            .WithMany(volunteer => volunteer.Shifts)
            .HasForeignKey(shift => shift.VolunteerId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<VolunteerShift>()
            .HasOne(shift => shift.PantrySite)
            .WithMany(site => site.VolunteerShifts)
            .HasForeignKey(shift => shift.PantrySiteId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryLot>().Property(lot => lot.QuantityReceived).HasConversion<double>();
        modelBuilder.Entity<InventoryLot>().Property(lot => lot.QuantityRemaining).HasConversion<double>();
        modelBuilder.Entity<DistributionLine>().Property(line => line.Quantity).HasConversion<double>();
    }
}
