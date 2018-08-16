using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using BestLot.DataAccessLayer.Entities;

namespace BestLot.DataAccessLayer.Contexts
{
    public class LotArchiveContext : DbContext
    {
        public LotArchiveContext(string connectionString) : base(connectionString)
        {
            Database.SetInitializer(new LotArchiveContextInitializer());
        }
        public DbSet<LotArchiveEntity> SoldLots { get; set; }
    }

    public class LotArchiveContextInitializer : CreateDatabaseIfNotExists<LotArchiveContext>
    {
        protected override void Seed(LotArchiveContext context)
        {
            base.Seed(context);
        }
    }
}
