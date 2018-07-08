using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.Entities;
using DataAccessLayer.Repository;
using DataAccessLayer.Contexts;

namespace DataAccessLayer.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(string lotContextConnectionString, string lotArchiveContextConnectionString)
        {
            LotContext = new LotContext(lotContextConnectionString);
        }

        private LotContext LotContext;
        private LotArchiveContext LotArchiveContext;

        private IRepository<Lot> _lots;
        private IRepository<Lot> _lotArchive;

        public IRepository<Lot> Lots { get { if (_lots == null) _lots = new GenericRepository<Lot>(LotContext); return _lots; } }
        public IRepository<Lot> LotArchive { get { if (_lotArchive == null) _lotArchive = new GenericRepository<Lot>(LotArchiveContext); return _lotArchive; } }

        public void RecreateDB()
        {
            LotContext.Database.Delete();
            LotContext.Database.Create();
        }

        public void RecreateArchive()
        {
            LotArchiveContext.Database.Delete();
            LotArchiveContext.Database.Create();
        }

        public void SaveChanges()
        {
            LotContext.SaveChanges();
        }

        public void SaveArchiveChanges()
        {
            LotArchiveContext.SaveChanges();
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
                    _lots.Dispose();
                    _lotArchive.Dispose();
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
