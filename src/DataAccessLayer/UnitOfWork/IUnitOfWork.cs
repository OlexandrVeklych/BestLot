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
        IRepository<LotEntity> Lots { get; }
        IRepository<LotEntity> LotArchive { get; }

        IRepository<UserAccountInfoEntity> UserAccounts { get; }

        void SaveChanges();
        void SaveArchiveChanges();

        void RecreateDB();
        void RecreateArchive();
    }
}
