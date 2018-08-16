using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestLot.DataAccessLayer.Entities;
using BestLot.DataAccessLayer.Repository;
using BestLot.DataAccessLayer.Contexts;

namespace BestLot.DataAccessLayer.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(string lotContextConnectionString, string lotArchiveContextConnectionString)
        {
            LotContext = new LotContext(lotContextConnectionString);
            LotArchiveContext = new LotArchiveContext(lotArchiveContextConnectionString);
        }

        private LotContext LotContext;
        private LotArchiveContext LotArchiveContext;

        private IRepository<LotEntity> _lots;
        private IRepository<LotArchiveEntity> _lotArchive;
        private IRepository<UserAccountInfoEntity> _userAccounts;
        private IRepository<LotCommentEntity> _lotComments;
        private IRepository<LotPhotoEntity> _lotPhotos;

        public IRepository<LotEntity> Lots { get { if (_lots == null) _lots = new GenericRepository<LotEntity>(LotContext); return _lots; } }
        public IRepository<LotArchiveEntity> LotArchive { get { if (_lotArchive == null) _lotArchive = new GenericRepository<LotArchiveEntity>(LotArchiveContext); return _lotArchive; } }
        public IRepository<UserAccountInfoEntity> UserAccounts { get { if (_userAccounts == null) _userAccounts = new GenericRepository<UserAccountInfoEntity>(LotContext); return _userAccounts; } }
        public IRepository<LotCommentEntity> LotComments { get { if (_lotComments == null) _lotComments = new GenericRepository<LotCommentEntity>(LotContext); return _lotComments; } }
        public IRepository<LotPhotoEntity> LotPhotos { get { if (_lotPhotos == null) _lotPhotos = new GenericRepository<LotPhotoEntity>(LotContext); return _lotPhotos; } }

        public void RecreateDB()
        {
            LotContext.Database.Delete();
            LotContext.Database.Create();
        }

        public async Task RecreateDBAsync()
        {
            await new Task(() => LotContext.Database.Delete());
            await new Task(() => LotContext.Database.Create());
        }

        public void RecreateArchive()
        {
            LotArchiveContext.Database.Delete();
            LotArchiveContext.Database.Create();
        }

        public void SaveChanges()
        {
            lock (LotContext)
            {
                LotContext.SaveChanges();
            }
        }

        public async Task SaveChangesAsync()
        {
            await LotContext.SaveChangesAsync();
        }

        public void SaveArchiveChanges()
        {
            lock (LotArchiveContext)
            {
                LotArchiveContext.SaveChanges();
            }
        }

        public async Task SaveArchiveChangesAsync()
        {
            await LotArchiveContext.SaveChangesAsync();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    LotContext.Dispose();
                    LotArchiveContext.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
