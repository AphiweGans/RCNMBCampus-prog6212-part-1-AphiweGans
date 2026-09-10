using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayContext : DbContext
{
    public RaceDayContext(DbContextOptions<RaceDayContext> options) : base(options) { }

    public DbSet<Organiser> Organisers => Set<Organiser>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unique emails across each user table
        modelBuilder.Entity<Organiser>()
            .HasIndex(o => o.Email)
            .IsUnique();

        modelBuilder.Entity<Participant>()
            .HasIndex(p => p.Email)
            .IsUnique();

        // Organiser 1 -- * Event
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser)
            .WithMany(o => o.Events)
            .HasForeignKey(e => e.OrganiserID)
            .OnDelete(DeleteBehavior.Restrict);

        // Event 1 -- * Category
        modelBuilder.Entity<Category>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventID)
            .OnDelete(DeleteBehavior.Cascade);

        // Participant 1 -- * Enrolment
        modelBuilder.Entity<Enrolment>()
            .HasOne(en => en.Participant)
            .WithMany(p => p.Enrolments)
            .HasForeignKey(en => en.ParticipantID)
            .OnDelete(DeleteBehavior.Restrict);

        // Category 1 -- * Enrolment
        modelBuilder.Entity<Enrolment>()
            .HasOne(en => en.Category)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(en => en.CategoryID)
            .OnDelete(DeleteBehavior.Restrict);

        // A participant may only enrol once per category
        modelBuilder.Entity<Enrolment>()
            .HasIndex(en => new { en.ParticipantID, en.CategoryID })
            .IsUnique();

        // Enrolment 1 -- 1 Result
        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrolment)
            .WithOne(en => en.Result)
            .HasForeignKey<Result>(r => r.EnrolmentID)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Result>()
            .HasIndex(r => r.EnrolmentID)
            .IsUnique();

        // Decimal precision (avoids SQL Server truncation warnings)
        modelBuilder.Entity<Event>().Property(e => e.Distance).HasPrecision(6, 2);
        modelBuilder.Entity<Category>().Property(c => c.DistanceKm).HasPrecision(5, 2);
        modelBuilder.Entity<Category>().Property(c => c.EntryFee).HasPrecision(8, 2);
    }
}
