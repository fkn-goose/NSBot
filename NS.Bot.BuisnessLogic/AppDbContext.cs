using Microsoft.EntityFrameworkCore;
using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Entities.Group;

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
        public DbSet<GroupMember> GroupMembers { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GroupMember>()
                .HasMany(e => e.Group)
                .WithMany(e => e.Members)
                .UsingEntity<MemberToGroup>(
                g => g.HasOne<GroupEntity>().WithMany().HasForeignKey(e => e.GroupId),
                m => m.HasOne<GroupMember>().WithMany().HasForeignKey(e => e.MemberId));

            modelBuilder.Entity<GroupEntity>()
                .HasMany(e => e.Members)
                .WithMany(e => e.Group)
                .UsingEntity<MemberToGroup>(
                m => m.HasOne<GroupMember>().WithMany().HasForeignKey(e => e.MemberId),
                g => g.HasOne<GroupEntity>().WithMany().HasForeignKey(e => e.GroupId));
        }
    }
}
