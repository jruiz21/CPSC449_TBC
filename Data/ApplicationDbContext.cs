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
    }
}
