using Grpc.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Grpc.Server.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<ToDoItem> ToDoItems { get; set; }
}