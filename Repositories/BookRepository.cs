using Microsoft.EntityFrameworkCore;
using TheBorrowedChapter.Data;
using TheBorrowedChapter.Models;

namespace TheBorrowedChapter.Repositories;

public class BookRepository : IBookRepository
{
    private readonly ApplicationDbContext _context;

    public BookRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        return await _context.Books.AsNoTracking().ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(Guid id)
    {
        return await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Book> AddAsync(Book book)
    {
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book> UpdateAsync(Book book)
    {
        _context.Books.Update(book);
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task DeleteAsync(Book book)
    {
        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(Guid id)
    {
        return _context.Books.AnyAsync(b => b.Id == id);
    }

    public Task<bool> IsbnExistsAsync(string isbn, Guid? excludeId = null)
    {
        return _context.Books.AnyAsync(b => b.ISBN == isbn && (excludeId == null || b.Id != excludeId));
    }
}
