using JudicialEvidence.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JudicialEvidence.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Case> Cases => Set<Case>();
    public DbSet<Evidence> Evidence => Set<Evidence>();
    public DbSet<EvidenceAdoption> EvidenceAdoptions => Set<EvidenceAdoption>();
    public DbSet<RetrievalLog> RetrievalLogs => Set<RetrievalLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(b =>
        {
            b.HasIndex(u => u.Username).IsUnique();
            b.Property(u => u.Role).HasDefaultValue(RoleNames.Police);
        });

        modelBuilder.Entity<Case>(b =>
        {
            b.HasIndex(c => c.CaseNumber).IsUnique();
            b.HasOne(c => c.Creator)
             .WithMany(u => u.Cases)
             .HasForeignKey(c => c.CreatedBy)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Evidence>(b =>
        {
            b.HasIndex(e => e.CaseId);
            b.HasOne(e => e.Case)
             .WithMany(c => c.Evidence)
             .HasForeignKey(e => e.CaseId)
             .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(e => e.Uploader)
             .WithMany(u => u.UploadedEvidence)
             .HasForeignKey(e => e.UploadedBy)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EvidenceAdoption>(b =>
        {
            b.HasIndex(a => a.EvidenceId);
            b.HasOne(a => a.Evidence)
             .WithMany(e => e.Adoptions)
             .HasForeignKey(a => a.EvidenceId)
             .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(a => a.Reviewer)
             .WithMany()
             .HasForeignKey(a => a.ReviewerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RetrievalLog>(b =>
        {
            b.HasIndex(r => r.CaseId);
            b.HasIndex(r => r.UserId);
            b.HasIndex(r => r.PurposeTag);
            b.Property(r => r.PurposeTag)
             .HasDefaultValue(RetrievalPurposeTag.CourtHearingExhibit);
            b.HasOne(r => r.Evidence)
             .WithMany(e => e.Retrievals)
             .HasForeignKey(r => r.EvidenceId)
             .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(r => r.Case)
             .WithMany(c => c.Retrievals)
             .HasForeignKey(r => r.CaseId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(r => r.User)
             .WithMany(u => u.Retrievals)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
