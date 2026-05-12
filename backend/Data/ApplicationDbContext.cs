using Microsoft.EntityFrameworkCore; // Resolve o DbContext
using AssistentVirtualPrivada.Models; // Resolve o ChatMessage

namespace AssistentVirtualPrivada.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChatMessage> ChatMessages { get; set; }
}