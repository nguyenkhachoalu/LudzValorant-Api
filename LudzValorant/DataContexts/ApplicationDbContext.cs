using LudzValorant.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LudzValorant.DataContexts
{
    public class ApplicationDbContext : DbContext, IDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountSkin> AccountSkins { get; set; }
        public DbSet<AccountAgent> AccountAgents { get; set; }
        public DbSet<AccountContract> AccountContracts { get; set; }
        public DbSet<AccountGunBuddy> AccountGunBuddies { get; set; }
        public DbSet<Weapon> Weapons { get; set; }
        public DbSet<Tier> Tiers { get; set; }
        public DbSet<Skin> Skins { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<GunBuddy> GunBuddies { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<InstallmentSchedule> InstallmentSchedules { get; set; }


        public async Task<int> CommitChangesAsync()
        {
            return await base.SaveChangesAsync();
        }

        public DbSet<TEntity> SetEntity<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<AccountSkin>()
                .HasKey(ur => new { ur.AccountId, ur.SkinId });
            modelBuilder.Entity<AccountContract>().HasKey(ac => new { ac.AccountId, ac.ContractId });
            modelBuilder.Entity<AccountGunBuddy>().HasKey(ag => new { ag.AccountId, ag.GunBuddyId });
            modelBuilder.Entity<AccountAgent>().HasKey(aa => new { aa.AccountId, aa.AgentId });


            modelBuilder.Entity<Product>()
                .HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict); // tránh multiple cascade path
        }
    }
}
