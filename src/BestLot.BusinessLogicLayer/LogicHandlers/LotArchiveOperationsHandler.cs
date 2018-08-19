using AutoMapper;
using AutoMapper.QueryableExtensions;
using BestLot.BusinessLogicLayer.Interfaces;
using BestLot.BusinessLogicLayer.Models;
using BestLot.DataAccessLayer.Entities;
using BestLot.DataAccessLayer.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.LogicHandlers
{
    public class LotArchiveOperationsHandler : ILotArchiveOperationsHandler
    {
        public LotArchiveOperationsHandler(IUnitOfWork unitOfWork)
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LotArchiveEntity, Lot>();
            }).CreateMapper();
            this.UoW = unitOfWork;
        }

        private IUnitOfWork UoW;
        private IMapper mapper;

        public IQueryable<Lot> GetUserArchivedLots(string userEmail)
        {
            Expression<Func<LotArchiveEntity, bool>> predicate = null;
            return UoW.LotArchive.GetAll().Where(predicate = lot => lot.SellerUserId == userEmail).ProjectTo<Lot>(mapper.ConfigurationProvider);
        }

        public async Task<IQueryable<Lot>> GetUserArchivedLotsAsync(string userEmail)
        {
            Expression<Func<LotArchiveEntity, bool>> predicate = null;
            return (await UoW.LotArchive.GetAllAsync()).Where(predicate = lot => lot.SellerUserId == userEmail).ProjectTo<Lot>(mapper.ConfigurationProvider);
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                UoW.Dispose();
                if (disposing)
                {
                    mapper = null;
                    UoW = null;
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
