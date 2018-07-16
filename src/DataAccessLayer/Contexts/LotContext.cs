using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using DataAccessLayer.Entities;

namespace DataAccessLayer.Contexts
{
    public class LotContext : DbContext
    {
        public LotContext(string connectionString) : base(connectionString) { }
        public DbSet<LotEntity> Lots { get; set; }
        public DbSet<UserAccountInfoEntity> UserAccounts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LotEntity>()
                .HasMany(lot => lot.Comments)
                .WithRequired(comment => comment.Lot)
                .HasForeignKey(comment => comment.LotId)
                .WillCascadeOnDelete();
            modelBuilder.Entity<LotEntity>()
                .HasMany(lot => lot.LotPhotos)
                .WithRequired(photo => photo.Lot)
                .HasForeignKey(photo => photo.LotId)
                .WillCascadeOnDelete();

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

            //modelBuilder.Entity<LotEntity>()
            //    .HasOptional(lot => lot.BuyerUser)
            //    .WithMany(user => user.Lots)
            //    .HasForeignKey(lot => lot.BuyerUserId);
            //
            //modelBuilder.Entity<LotEntity>()
            //    .HasRequired(lot => lot.SellerUser)
            //    .WithMany(user => user.Lots)
            //    .HasForeignKey(lot => lot.SellerUserId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
