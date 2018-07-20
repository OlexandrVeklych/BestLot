using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using DataAccessLayer.Entities;

namespace DataAccessLayer.Contexts
{
    public class LotArchiveContext : DbContext
    {
        public LotArchiveContext(string connectionString) : base(connectionString) { }
        public DbSet<ArchiveLotEntity> SoldLots { get; set; }
    }
}
