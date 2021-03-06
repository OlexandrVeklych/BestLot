﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BestLot.BusinessLogicLayer.Interfaces;
using BestLot.BusinessLogicLayer.Models;
using BestLot.BusinessLogicLayer.Exceptions;
using BestLot.DataAccessLayer.Entities;
using BestLot.DataAccessLayer.UnitOfWork;

namespace BestLot.BusinessLogicLayer.LogicHandlers
{
    public class LotCommentOperationsHandler : ILotCommentOperationsHandler
    {
        public LotCommentOperationsHandler(IUnitOfWork unitOfWork, ILotOperationsHandler lotOperationsHandler, IUserAccountOperationsHandler userAccountOperationsHandler)
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
            //Don`t check id`s, because
            //This will throw exception if UserId is wrong
            userAccountOperationsHandler.GetUserAccount(lotComment.UserId);
            //This will throw exception if LotId if wrong
            Lot lot = lotOperationsHandler.GetLot(lotComment.LotId);
            lot.LotComments = GetLotComments(lot.Id).ToList();
            lot.AddComment(lotComment);
            UoW.LotComments.Add(mapper.Map<LotCommentEntity>(lotComment));
            UoW.SaveChanges();
            //private ChangeLot, without checking Lot again
            lotOperationsHandler.ChangeLotUnsafe(lot);
        }

        public async Task AddCommentAsync(LotComment lotComment)
        {
            //Don`t check id`s, because
            //This will throw exception if UserId is wrong
            await userAccountOperationsHandler.GetUserAccountAsync(lotComment.UserId);
            //This will throw exception if LotId if wrong
            Lot lot = await lotOperationsHandler.GetLotAsync(lotComment.LotId);
            lot.LotComments = (await GetLotCommentsAsync(lot.Id)).ToList();
            lot.AddComment(lotComment);
            UoW.LotComments.Add(mapper.Map<LotCommentEntity>(lotComment));
            await UoW.SaveChangesAsync();
            //private ChangeLot, without checking Lot again
            await lotOperationsHandler.ChangeLotUnsafeAsync(lot);
        }

        public LotComment GetLotCommentByPosition(int lotId, int lotCommentPosition)
        {
            List<LotComment> lotComments = GetLotComments(lotId).ToList();
            if (lotComments.Count() <= lotCommentPosition)
                throw new WrongModelException("No comment on that position");
            return lotComments[lotCommentPosition];
        }

        public async Task<LotComment> GetLotCommentByPositionAsync(int lotId, int lotCommentPosition)
        {
            List<LotComment> lotComments = (await GetLotCommentsAsync(lotId)).ToList();
            if (lotComments.Count() <= lotCommentPosition)
                throw new WrongModelException("No comment on that position");
            return lotComments[lotCommentPosition];
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

        public IQueryable<LotComment> GetUserComments(string userEmail)
        {
            Expression<Func<LotCommentEntity, bool>> predicate = null;
            return UoW.LotComments.GetAll().Where(predicate = lotComment => lotComment.UserId == userEmail).ProjectTo<LotComment>(mapper.ConfigurationProvider);
        }

        public async Task<IQueryable<LotComment>> GetUserCommentsAsync(string userEmail)
        {
            Expression<Func<LotCommentEntity, bool>> predicate = null;
            return (await UoW.LotComments.GetAllAsync()).Where(predicate = lotComment => lotComment.UserId == userEmail).ProjectTo<LotComment>(mapper.ConfigurationProvider);
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                UoW.Dispose();
                lotOperationsHandler.Dispose();
                userAccountOperationsHandler.Dispose();
                if (disposing)
                {
                    lotOperationsHandler = null;
                    userAccountOperationsHandler = null;
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
