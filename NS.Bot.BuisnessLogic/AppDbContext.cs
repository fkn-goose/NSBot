using Microsoft.EntityFrameworkCore;
using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Entities.Group;
using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Entities.Radio;
using NS.Bot.Shared.Entities.Warn;

namespace NS.Bot.BuisnessLogic
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
            ChangeTracker.LazyLoadingEnabled = false;
        }

        public DbSet<MemberEntity> Members { get; set; }

        #region Ticket

        public DbSet<TicketSettings> TicketSettings { get; set; }
        public DbSet<TicketEntity> Tickets { get; set; }

        #endregion

        #region Groups

        public DbSet<GroupEntity> Groups { get; set; }

        #endregion

        #region Guild

        public DbSet<GuildMember> GuildMembers { get; set; }
        public DbSet<GuildEntity> Guilds { get; set; }

        #endregion

        #region Radio

        public DbSet<RadioEntity> Radios { get; set; }
        public DbSet<RadioSettings> RadioSettings { get; set; }

        #endregion

        #region Warn

        public DbSet<WarnEntity> Warns { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GroupEntity>(g =>
            {
                g.HasIndex(u => u.Name)
                .IsUnique();
            });
        }
    }
}
