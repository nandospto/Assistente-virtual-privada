using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options){}

        public DbSet<Chat> Chat {get; set;}
        public DbSet<Configuracoes> Configuracoes {get; set;}
        public DbSet<Historico_Conversa> Historico_Conversa {get; set;}
        public DbSet<Notas> Notas {get; set;}
    }
}