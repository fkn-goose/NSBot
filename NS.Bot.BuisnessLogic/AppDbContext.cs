using Microsoft.EntityFrameworkCore;
using NS.Bot.Shared.Entities;

namespace NS.Bot.BuisnessLogic
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<GuildEntity> Guilds { get; set; }
        public DbSet<TicketSettings> TicketSettings { get; set; }
        public DbSet<TicketEntity> Tickets { get; set; }
    } 
}
