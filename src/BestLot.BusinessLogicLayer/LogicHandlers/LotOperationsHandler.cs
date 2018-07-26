﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestLot.DataAccessLayer.UnitOfWork;
using BestLot.DataAccessLayer.Entities;
using BestLot.BusinessLogicLayer.Models;
using AutoMapper;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;

namespace BestLot.BusinessLogicLayer.LogicHandlers
{
    public class LotOperationsHandler : ILotOperationsHandler
    {
        public LotOperationsHandler(IUnitOfWork unitOfWork)
        {
            UoW = unitOfWork;
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

        private IUnitOfWork UoW;
        private IMapper mapper;

        public void AddLot(Lot lot)
        {
            if (UoW.UserAccounts.Get(lot.SellerUserId) == null)
                throw new ArgumentException("Seller user id is incorrect");
            lot.StartDate = DateTime.Now;
            lot.BuyerUserId = null;
            UoW.Lots.Add(mapper.Map<LotEntity>(lot));
            UoW.SaveChanges();
        }

        public async Task AddLotAsync(Lot lot)
        {
            if (await UoW.UserAccounts.GetAsync(lot.SellerUserId) == null)
                throw new ArgumentException("Seller user id is incorrect");
            lot.StartDate = DateTime.Now;
            lot.BuyerUserId = null;
            UoW.Lots.Add(mapper.Map<LotEntity>(lot));
            await UoW.SaveChangesAsync();
        }

        public void ChangeLot(int id, Lot newLot)
        {
            if (UoW.Lots.Get(id) == null)
                AddLot(newLot);
            else
            {
                Lot currentLot = mapper.Map<Lot>(UoW.Lots.Get(id));
                if (currentLot.Id != newLot.Id
                    || (currentLot.BuyerUserId != null && currentLot.Price != newLot.Price)
                    || currentLot.SellerUserId != newLot.SellerUserId
                    || currentLot.BuyerUserId != newLot.BuyerUserId)
                    throw new ArgumentException("No permission to change these properties");
                UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
                UoW.SaveChanges();
            }
        }

        public async Task ChangeLotAsync(int id, Lot newLot)
        {
            if (await UoW.Lots.GetAsync(id) == null)
                await AddLotAsync(newLot);
            else
            {
                Lot currentLot = mapper.Map<Lot>(await UoW.Lots.GetAsync(id));
                if (currentLot.Id != newLot.Id
                    || (currentLot.BuyerUserId != null && currentLot.Price != newLot.Price)
                    || currentLot.SellerUserId != newLot.SellerUserId
                    || currentLot.BuyerUserId != newLot.BuyerUserId)
                    throw new ArgumentException("No permission to change these properties");
                UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
                await UoW.SaveChangesAsync();
            }
        }

        //id of newLot is correct, don`t check it again
        private void ChangeLot(Lot newLot)
        {
            UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
            UoW.SaveChanges();
        }

        //id of newLot is correct, don`t check it again
        private async Task ChangeLotAsync(Lot newLot)
        {
            UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
            await UoW.SaveChangesAsync();
        }

        public void DeleteLot(int lotId)
        {
            if (UoW.Lots.Get(lotId) == null)
                throw new ArgumentException("Lot id is incorrect");
            UoW.Lots.Delete(lotId);
            UoW.SaveChanges();
        }

        public async Task DeleteLotAsync(int lotId)
        {
            if (await UoW.Lots.GetAsync(lotId) == null)
                throw new ArgumentException("Lot id is incorrect");
            UoW.Lots.Delete(lotId);
            await UoW.SaveChangesAsync();
        }

        public Lot GetLot(int lotId, params Expression<Func<Lot, object>>[] includeProperties)
        {
            Lot lot = mapper.Map<Lot>(UoW.Lots.Get(lotId, mapper.Map<Expression<Func<LotEntity, object>>[]>(includeProperties)));
            if (lot == null)
                throw new ArgumentException("Lot id is incorrect");
            return lot;
        }

        public async Task<Lot> GetLotAsync(int lotId, params Expression<Func<Lot, object>>[] includeProperties)
        {
            Lot lot = mapper.Map<Lot>(await UoW.Lots.GetAsync(lotId, mapper.Map<Expression<Func<LotEntity, object>>[]>(includeProperties)));
            if (lot == null)
                throw new ArgumentException("Lot id is incorrect");
            return lot;
        }

        public IQueryable<Lot> GetAllLots(params Expression<Func<Lot, object>>[] includeProperties)
        {
            return UoW.Lots.GetAll(mapper.Map<Expression<Func<LotEntity, object>>[]>(includeProperties)).ProjectTo<Lot>(mapper.ConfigurationProvider);
        }

        public async Task<IQueryable<Lot>> GetAllLotsAsync(params Expression<Func<Lot, object>>[] includeProperties)
        {
            var result = await UoW.Lots.GetAllAsync(mapper.Map<Expression<Func<LotEntity, object>>[]>(includeProperties));
            return result.ProjectTo<Lot>(mapper.ConfigurationProvider);
        }

        public void PlaceBet(string buyerUserId, int lotId, double price)
        {
            if (UoW.UserAccounts.Get(buyerUserId) == null)
                throw new ArgumentException("User id is incorrect");
            Lot lot = mapper.Map<Lot>(UoW.Lots.Get(lotId, l => l.LotPhotos, l => l.LotComments, l => l.SellerUser));
            if (lot == null)
                throw new ArgumentException("Lot id is incorrect");
            if (price < lot.Price + lot.MinStep)
                throw new ArgumentException("Your bet can be " + (lot.Price + lot.MinStep) + " or higher");
            lot.BuyerUserId = buyerUserId;
            lot.Price = price;

            //private ChangeLot, without checking LotId
            ChangeLot(lot);
        }

        public async Task PlaceBetAsync(string buyerUserId, int lotId, double price)
        {
            if (await UoW.UserAccounts.GetAsync(buyerUserId) == null)
                throw new ArgumentException("User id is incorrect");
            Lot lot = mapper.Map<Lot>(await UoW.Lots.GetAsync(lotId, l => l.LotPhotos, l => l.LotComments, l => l.SellerUser));
            if (lot == null)
                throw new ArgumentException("Lot id is incorrect");
            if (price < lot.Price + lot.MinStep)
                throw new ArgumentException("Your bet can be " + (lot.Price + lot.MinStep) + " or higher");
            lot.BuyerUserId = buyerUserId;
            lot.Price = price;

            //private ChangeLot, without checking LotId
            await ChangeLotAsync(lot);
        }

        public void AddComment(LotComment lotComment)
        {
            if (UoW.UserAccounts.Get(lotComment.UserId) == null)
                throw new ArgumentException("User id is incorrect");
            Lot lot = mapper.Map<Lot>(UoW.Lots.Get(lotComment.LotId, l => l.LotPhotos, l => l.LotComments, l => l.SellerUser));
            if (lot == null)
                throw new ArgumentException("Lot id is incorrect");
            lot.AddComment(lotComment);
            UoW.LotComments.Add(mapper.Map<LotCommentEntity>(lotComment));
            //private ChangeLot, without checking LotId
            ChangeLot(lot);
        }

        public async Task AddCommentAsync(LotComment lotComment)
        {
            if (await UoW.UserAccounts.GetAsync(lotComment.UserId) == null)
                throw new ArgumentException("User id is incorrect");
            Lot lot = mapper.Map<Lot>(await UoW.Lots.GetAsync(lotComment.LotId, l => l.LotPhotos, l => l.LotComments, l => l.SellerUser));
            if (lot == null)
                throw new ArgumentException("Lot id is incorrect");
            lot.AddComment(lotComment);
            UoW.LotComments.Add(mapper.Map<LotCommentEntity>(lotComment));
            //private ChangeLot, without checking LotId
            await ChangeLotAsync(lot);
        }
    }
}
