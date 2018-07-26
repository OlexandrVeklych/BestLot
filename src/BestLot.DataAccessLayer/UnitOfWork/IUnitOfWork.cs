using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestLot.DataAccessLayer.Repository;
using BestLot.DataAccessLayer.Entities;

namespace BestLot.DataAccessLayer.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<LotEntity> Lots { get; }
        IRepository<ArchiveLotEntity> LotArchive { get; }
        IRepository<LotCommentEntity> LotComments { get; }

        IRepository<UserAccountInfoEntity> UserAccounts { get; }

        void SaveChanges();
        Task SaveChangesAsync();
        void SaveArchiveChanges();

        void RecreateDB();
        Task RecreateDBAsync();
        void RecreateArchive();
    }
}
