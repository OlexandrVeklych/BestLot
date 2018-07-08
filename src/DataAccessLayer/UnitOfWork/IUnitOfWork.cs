using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.Repository;
using DataAccessLayer.Entities;

namespace DataAccessLayer.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Lot> Lots { get; }
        IRepository<Lot> LotArchive { get; }

        void SaveChanges();
        void SaveArchiveChanges();

        void RecreateDB();
        void RecreateArchive();
    }
}
