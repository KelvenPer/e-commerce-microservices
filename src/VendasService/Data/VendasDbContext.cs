using Microsoft.EntityFrameworkCore;
using VendasService.Models;

namespace VendasService.Data
{
    public class VendasDbContext : DbContext
    {
        public VendasDbContext(DbContextOptions<VendasDbContext> options)
            : base(options)
        {
        }
        public DbSet<Pedido> Pedidos { get; set; }
    }
}