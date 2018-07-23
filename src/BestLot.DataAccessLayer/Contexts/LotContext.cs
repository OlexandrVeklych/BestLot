using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using BestLot.DataAccessLayer.Entities;

namespace BestLot.DataAccessLayer.Contexts
{
    public class LotContext : DbContext
    {
        public LotContext(string connectionString) : base(connectionString) { }
        public DbSet<LotEntity> Lots { get; set; }
        public DbSet<UserAccountInfoEntity> UserAccounts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LotEntity>()
                .HasMany(lot => lot.LotComments)
                .WithRequired(comment => comment.Lot)
                .HasForeignKey(comment => comment.LotId)
                .WillCascadeOnDelete();

            modelBuilder.Entity<LotEntity>()
                .HasMany(lot => lot.LotPhotos)
                .WithRequired(photo => photo.Lot)
                .HasForeignKey(photo => photo.LotId)
                .WillCascadeOnDelete();

            modelBuilder.Entity<UserAccountInfoEntity>()
                .HasKey(user => user.Email)
                .Property(user => user.Email)
                .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);

            modelBuilder.Entity<UserAccountInfoEntity>()
                .HasMany(user => user.Lots)
                .WithRequired(lot => lot.SellerUser)
                .HasForeignKey(lot => lot.SellerUserId)
                .WillCascadeOnDelete();

            modelBuilder.Entity<UserAccountInfoEntity>()
               .HasMany(user => user.LotComments)
               .WithRequired(comment => comment.User)
               .HasForeignKey(comment => comment.UserId)
               .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
