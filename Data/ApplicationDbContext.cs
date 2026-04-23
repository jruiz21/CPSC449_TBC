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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasIndex(b => b.ISBN).IsUnique();
        });
    }
}
