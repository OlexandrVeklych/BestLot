using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.UnitOfWork;
using DataAccessLayer.Entities;
using BusinessLogicLayer.Models;
using AutoMapper;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;

namespace BusinessLogicLayer.LogicHandlers
{
    public class LotOperationsHandler
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
                cfg.CreateMap<UserAccountInfo, UserAccountInfoEntity>().MaxDepth(1);
                cfg.CreateMap<UserAccountInfoEntity, UserAccountInfo>();
                cfg.CreateMap<Expression<Func<Lot, object>>[], Expression<Func<LotEntity, object>>[]>();
                cfg.CreateMap<Func<Lot, bool>, Func<LotEntity, bool>>();
                cfg.CreateMap<Func<Lot, object>, Func<LotEntity, object>>();
            }).CreateMapper();
        }

        private IUnitOfWork UoW;
        private IMapper mapper;

        public void AddLot(Lot lot)
        {
            if (UoW.UserAccounts.Get(lot.SellerUserId) == null)
                throw new ArgumentException("Seller user id is incorrect");
            lot.StartDate = DateTime.Now;
            UoW.Lots.Add(mapper.Map<LotEntity>(lot));
            UoW.SaveChanges();
        }

        public void ChangeLot(int id, Lot newLot)
        {
            if (UoW.Lots.Get(id) == null)
                throw new ArgumentException("Lot id is incorrect");
            Lot currentLot = mapper.Map<Lot>(UoW.Lots.Get(id));
            if (currentLot.Id != newLot.Id
                || (currentLot.BuyerUserId != 0 && currentLot.Price != newLot.Price)
                || currentLot.SellerUserId != newLot.SellerUserId
                || currentLot.BuyerUserId != newLot.BuyerUserId)
                throw new ArgumentException("No permission to change these properties");
            UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
            UoW.SaveChanges();
        }

        //id of newLot is correct, don`t check it again
        private void ChangeLot(Lot newLot)
        {
            UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
            UoW.SaveChanges();
        }

        public void DeleteLot(int lotId)
        {
            if (UoW.Lots.Get(lotId) == null)
                throw new ArgumentException("Lot id is incorrect");
            UoW.Lots.Delete(lotId);
            UoW.SaveChanges();
        }

        public Lot GetLot(int lotId)
        {
            if (UoW.Lots.Get(lotId) == null)
                throw new ArgumentException("Lot id is incorrect");
            return mapper.Map<Lot>(UoW.Lots.Get(lotId));
        }

        public Lot GetLot(int lotId, params Expression<Func<Lot, object>>[] includeProperties)
        {
            if (UoW.Lots.Get(lotId) == null)
                throw new ArgumentException("Lot id is incorrect");
            return mapper.Map<Lot>(UoW.Lots.Get(lotId, mapper.Map<Expression<Func<LotEntity, object>>[]>(includeProperties)));
        }

        public IQueryable<Lot> GetAllLots()
        {
            return UoW.Lots.GetAll().ProjectTo<Lot>(mapper.ConfigurationProvider);
        }

        public IQueryable<Lot> GetAllLots(params Expression<Func<Lot, object>>[] includeProperties)
        {
            return UoW.Lots.GetAll(mapper.Map<Expression<Func<LotEntity, object>>[]>(includeProperties)).ProjectTo<Lot>(mapper.ConfigurationProvider);
        }

        public IQueryable<Lot> GetAllLots(Func<Lot, bool> predicate, params Expression<Func<Lot, object>>[] includeProperties)
        {
            return GetAllLots(includeProperties).Where(predicate).AsQueryable();
        }

        public void PlaceBet(int buyerUserId, int lotId, double price)
        {
            if (UoW.UserAccounts.Get(buyerUserId) == null)
                throw new ArgumentException("User id is incorrect");
            if (UoW.Lots.Get(lotId) == null)
                throw new ArgumentException("Lot id is incorrect");
            Lot lot = mapper.Map<Lot>(UoW.Lots.Get(lotId, l => l.LotPhotos, l => l.Comments, l => l.SellerUser));
            if (price < lot.Price + lot.MinStep)
                throw new ArgumentException("Your bet can be " + (lot.Price + lot.MinStep) + " or higher");
            lot.BuyerUserId = buyerUserId;
            lot.Price = price;

            //private ChangeLot, without checking LotId
            ChangeLot(lot);
        }

        public void AddComment(LotComment lotComment)
        {
            if (UoW.UserAccounts.Get(lotComment.UserId) == null)
                throw new ArgumentException("User id is incorrect");
            if (UoW.Lots.Get(lotComment.LotId) == null)
                throw new ArgumentException("Lot id is incorrect");
            Lot lot = mapper.Map<Lot>(UoW.Lots.Get(lotComment.LotId, l => l.LotPhotos, l => l.Comments, l => l.SellerUser));
            lot.AddComment(lotComment);
            UoW.LotComments.Add(mapper.Map<LotCommentEntity>(lotComment));
            //private ChangeLot, without checking LotId
            ChangeLot(lot);
        }
    }
}
