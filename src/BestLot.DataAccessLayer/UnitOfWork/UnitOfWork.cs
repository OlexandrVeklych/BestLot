﻿using System;
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
            LotArchiveContext = new LotArchiveContext(lotArchiveContextConnectionString);
        }

        private LotContext LotContext;
        private LotArchiveContext LotArchiveContext;

        private IRepository<LotEntity> _lots;
        private IRepository<ArchiveLotEntity> _lotArchive;
        private IRepository<UserAccountInfoEntity> _userAccounts;
        private IRepository<LotCommentEntity> _lotComments;

        public IRepository<LotEntity> Lots { get { if (_lots == null) _lots = new GenericRepository<LotEntity>(LotContext); return _lots; } }
        public IRepository<ArchiveLotEntity> LotArchive { get { if (_lotArchive == null) _lotArchive = new GenericRepository<ArchiveLotEntity>(LotArchiveContext); return _lotArchive; } }
        public IRepository<UserAccountInfoEntity> UserAccounts { get { if (_userAccounts == null) _userAccounts = new GenericRepository<UserAccountInfoEntity>(LotContext); return _userAccounts; } }
        public IRepository<LotCommentEntity> LotComments { get { if (_lotComments == null) _lotComments = new GenericRepository<LotCommentEntity>(LotContext); return _lotComments; } }

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
            //try
            //{
            //    LotContext.SaveChanges();
            //}
            //catch(System.Data.Entity.Infrastructure.DbUpdateException)
            //{
            //    throw new ArgumentException("Wrong id");
            //}
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