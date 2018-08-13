using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
                cfg.CreateMap<LotEntity, Lot>()
                .ForAllMembers(opt => opt.Ignore());
                cfg.CreateMap<UserAccountInfoEntity, UserAccountInfo>()
                .ForMember(dest => dest.Lots, opt => opt.Ignore())
                .ForMember(dest => dest.LotComments, opt => opt.Ignore());
                cfg.CreateMap<LotPhotoEntity, LotPhoto>()
                .ForAllMembers(opt => opt.Ignore());
                cfg.CreateMap<LotCommentEntity, LotComment>();

                cfg.CreateMap<LotComment, LotCommentEntity>();

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
            lot.LotComments = GetLotComments(lot.Id).ToList();
            lot.AddComment(lotComment);
            UoW.LotComments.Add(mapper.Map<LotCommentEntity>(lotComment));
            UoW.SaveChanges();
            //private ChangeLot, without checking Lot again
            lotOperationsHandler.ChangeLotUnsafe(lot);
        }

        public async Task AddCommentAsync(LotComment lotComment)
        {
            if (await userAccountOperationsHandler.GetUserAccountAsync(lotComment.UserId) == null)
                throw new ArgumentException("User id is incorrect");
            Lot lot;
            if ((lot = await lotOperationsHandler.GetLotAsync(lotComment.LotId)) == null)
                throw new ArgumentException("Lot id is incorrect");
            lot.LotComments = (await GetLotCommentsAsync(lot.Id)).ToList();
            lot.AddComment(lotComment);
            UoW.LotComments.Add(mapper.Map<LotCommentEntity>(lotComment));
            await UoW.SaveChangesAsync();
            //private ChangeLot, without checking Lot again
            await lotOperationsHandler.ChangeLotUnsafeAsync(lot);
        }

        public IQueryable<LotComment> GetLotComments(int lotId)
        {
            Expression<Func<LotCommentEntity, bool>> predicate = null;
            return UoW.LotComments.GetAll().Where(predicate = lotComment => lotComment.LotId == lotId).ProjectTo<LotComment>(mapper.ConfigurationProvider);
        }

        public async Task<IQueryable<LotComment>> GetLotCommentsAsync(int lotId)
        {
            Expression<Func<LotCommentEntity, bool>> predicate = null;
            return (await UoW.LotComments.GetAllAsync()).Where(predicate = lotComment => lotComment.LotId == lotId).ProjectTo<LotComment>(mapper.ConfigurationProvider);
        }

        public IQueryable<LotComment> GetUserComments(string userId)
        {
            Expression<Func<LotCommentEntity, bool>> predicate = null;
            return UoW.LotComments.GetAll().Where(predicate = lotComment => lotComment.UserId == userId).ProjectTo<LotComment>(mapper.ConfigurationProvider);
        }

        public async Task<IQueryable<LotComment>> GetUserCommentsAsync(string userId)
        {
            Expression<Func<LotCommentEntity, bool>> predicate = null;
            return (await UoW.LotComments.GetAllAsync()).Where(predicate = lotComment => lotComment.UserId == userId).ProjectTo<LotComment>(mapper.ConfigurationProvider);
        }
    }
}
