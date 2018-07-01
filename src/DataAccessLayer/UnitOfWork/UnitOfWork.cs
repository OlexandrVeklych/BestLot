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
        public UnitOfWork(string lotContextConnectionString)
        {
            LotContext = new LotContext(lotContextConnectionString);
        }

        private LotContext LotContext;

        private IRepository<Lot> _lots;

        public IRepository<Lot> Lots { get { if (_lots == null) _lots = new GenericRepository<Lot>(LotContext); return _lots; } }

        public void RecreateDB()
        {
            LotContext.Database.Delete();
            LotContext.Database.Create();
        }

        public void SaveChanges()
        {
            LotContext.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    LotContext.Dispose();
                    _lots.Dispose();
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
