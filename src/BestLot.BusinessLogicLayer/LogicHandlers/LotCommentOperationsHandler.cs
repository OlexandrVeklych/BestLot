using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BestLot.BusinessLogicLayer;
using BestLot.BusinessLogicLayer.Interfaces;
using BestLot.BusinessLogicLayer.Models;
using BestLot.DataAccessLayer.Entities;
using BestLot.DataAccessLayer.UnitOfWork;

namespace BestLot.BusinessLogicLayer.LogicHandlers
{
    public class LotCommentOperationsHandler : ILotCommentOperationsHandler
    {
        private LotCommentOperationsHandler()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                //MaxDepth(1) - to map User inside Lot without his Lots
                //Lot.User.Name - ok
                //Lot.User.Lots[n] - null reference
                cfg.CreateMap<LotEntity, Lot>().MaxDepth(1);
                cfg.CreateMap<Lot, LotEntity>();
                //MaxDepth(1) - to map User inside Comment without his Lots
                //LotComment.User.Name - ok
                //LotComment.User.Lots[n] - null reference
                cfg.CreateMap<LotCommentEntity, LotComment>().MaxDepth(1);
                cfg.CreateMap<LotComment, LotCommentEntity>();
                cfg.CreateMap<LotPhotoEntity, LotPhoto>();
                cfg.CreateMap<LotPhoto, LotPhotoEntity>();
                cfg.CreateMap<UserAccountInfo, UserAccountInfoEntity>();
                cfg.CreateMap<UserAccountInfoEntity, UserAccountInfo>();
                cfg.CreateMap<Expression<Func<Lot, object>>[], Expression<Func<LotEntity, object>>[]>();
            }).CreateMapper();
        }

        public LotCommentOperationsHandler(IUnitOfWork unitOfWork, ILotOperationsHandler lotOperationsHandler, IUserAccountOperationsHandler userAccountOperationsHandler) : this()
        {
            this.UoW = unitOfWork;
            this.lotOperationsHandler = lotOperationsHandler;
            this.userAccountOperationsHandler = userAccountOperationsHandler;
        }

        private ILotOperationsHandler lotOperationsHandler;
        private IUserAccountOperationsHandler userAccountOperationsHandler;
        private IUnitOfWork UoW;
        private IMapper mapper;

        public void AddComment(LotComment lotComment)
        {
            if (userAccountOperationsHandler.GetUserAccount(lotComment.UserId) == null)
                throw new ArgumentException("User id is incorrect");
            Lot lot;
            if ((lot = lotOperationsHandler.GetLot(lotComment.LotId)) == null)
                throw new ArgumentException("Lot id is incorrect");
            lot.AddComment(lotComment);
            UoW.LotComments.Add(mapper.Map<LotCommentEntity>(lotComment));
            UoW.SaveChanges();
            //private ChangeLot, without checking LotId
            lotOperationsHandler.ChangeLotUnsafe(lot);
        }

        public async Task AddCommentAsync(LotComment lotComment)
        {
            if (await userAccountOperationsHandler.GetUserAccountAsync(lotComment.UserId) == null)
                throw new ArgumentException("User id is incorrect");
            Lot lot;
            if ((lot = await lotOperationsHandler.GetLotAsync(lotComment.LotId)) == null)
                throw new ArgumentException("Lot id is incorrect");
            lot.AddComment(lotComment);
            UoW.LotComments.Add(mapper.Map<LotCommentEntity>(lotComment));
            await UoW.SaveChangesAsync();
            //private ChangeLot, without checking LotId
            await lotOperationsHandler.ChangeLotUnsafeAsync(lot);
        }

        public IQueryable<LotComment> GetLotComments(int lotId, params Expression<Func<Lot, object>>[] includeProperties)
        {
            Expression<Func<LotCommentEntity, bool>> predicate = null;
            return UoW.LotComments.GetAll().Where(predicate = lotComment => lotComment.LotId == lotId).ProjectTo<LotComment>(mapper.ConfigurationProvider);
        }

        public async Task<IQueryable<LotComment>> GetLotCommentsAsync(int lotId, params Expression<Func<Lot, object>>[] includeProperties)
        {
            Expression<Func<LotCommentEntity, bool>> predicate = null;
            return (await UoW.LotComments.GetAllAsync()).Where(predicate = lotComment => lotComment.LotId == lotId).ProjectTo<LotComment>(mapper.ConfigurationProvider);
        }

        public IQueryable<LotComment> GetUserComments(string userId, params Expression<Func<Lot, object>>[] includeProperties)
        {
            Expression<Func<LotCommentEntity, bool>> predicate = null;
            return UoW.LotComments.GetAll().Where(predicate = lotComment => lotComment.UserId == userId).ProjectTo<LotComment>(mapper.ConfigurationProvider);
        }

        public async Task<IQueryable<LotComment>> GetUserCommentsAsync(string userId, params Expression<Func<Lot, object>>[] includeProperties)
        {
            Expression<Func<LotCommentEntity, bool>> predicate = null;
            return (await UoW.LotComments.GetAllAsync()).Where(predicate = lotComment => lotComment.UserId == userId).ProjectTo<LotComment>(mapper.ConfigurationProvider);
        }
    }
}
