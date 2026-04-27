using Microsoft.EntityFrameworkCore;
using TheBorrowedChapter.Models;

namespace TheBorrowedChapter.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<BorrowRecord> BorrowRecords => Set<BorrowRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasIndex(b => b.ISBN).IsUnique();
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasIndex(m => m.Email).IsUnique();
        });

        modelBuilder.Entity<BorrowRecord>(entity =>
        {
            entity.HasIndex(r => new { r.MemberId, r.BorrowDate });
            entity.HasIndex(r => new { r.BookId, r.MemberId, r.Status });

            entity.Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(r => r.Book)
                .WithMany()
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Member)
                .WithMany()
                .HasForeignKey(r => r.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
