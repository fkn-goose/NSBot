using Microsoft.EntityFrameworkCore;
using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Entities.Group;
using NS.Bot.Shared.Entities.Guild;

namespace NS.Bot.BuisnessLogic
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<GuildEntity> Guilds { get; set; }

        #region Ticket

        public DbSet<TicketSettings> TicketSettings { get; set; }
        public DbSet<TicketEntity> Tickets { get; set; }

        #endregion

        #region Groups

        public DbSet<GroupEntity> Groups { get; set; }
        public DbSet<GuildMember> GroupMembers { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GroupEntity>(g =>
            {
                g.HasIndex(u => u.Group)
                .IsUnique();
            });
        }
    }
}
