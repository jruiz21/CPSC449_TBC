using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using TheBorrowedChapter.Data;
using TheBorrowedChapter.Dtos;
using TheBorrowedChapter.Repositories;
using TheBorrowedChapter.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .ToArray();

        var message = errors.Length > 0
            ? string.Join(" ", errors)
            : "The request is invalid.";

        return new BadRequestObjectResult(new ErrorResponse(message));
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IBorrowRepository, BorrowRepository>();
builder.Services.AddScoped<IBorrowService, BorrowService>();

var app = builder.Build();

// Auto-create DB on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
    db.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS BorrowRecords (
            Id TEXT NOT NULL CONSTRAINT PK_BorrowRecords PRIMARY KEY,
            BookId TEXT NOT NULL,
            MemberId TEXT NOT NULL,
            BorrowDate TEXT NOT NULL,
            ReturnDate TEXT NULL,
            Status TEXT NOT NULL,
            CONSTRAINT FK_BorrowRecords_Books_BookId FOREIGN KEY (BookId) REFERENCES Books (Id) ON DELETE RESTRICT,
            CONSTRAINT FK_BorrowRecords_Members_MemberId FOREIGN KEY (MemberId) REFERENCES Members (Id) ON DELETE RESTRICT
        );
        """);
    db.Database.ExecuteSqlRaw("""
        CREATE INDEX IF NOT EXISTS IX_BorrowRecords_MemberId_BorrowDate
        ON BorrowRecords (MemberId, BorrowDate);
        """);
    db.Database.ExecuteSqlRaw("""
        CREATE INDEX IF NOT EXISTS IX_BorrowRecords_BookId_MemberId_Status
        ON BorrowRecords (BookId, MemberId, Status);
        """);
}

// Global exception handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Unhandled exception");
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = new ErrorResponse("An unexpected error occurred.");
        await context.Response.WriteAsJsonAsync(error);
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
