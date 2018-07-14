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

            base.OnModelCreating(modelBuilder);
        }
    }
}
