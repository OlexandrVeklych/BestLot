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

namespace BusinessLogicLayer
{
    public class LotOperationsController
    {
        public LotOperationsController(IUnitOfWork unitOfWork)
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
                //LotCommand.User.Name - ok
                //LotCommand.User.Lots[n] - null reference
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
            UoW.Lots.Add(mapper.Map<LotEntity>(lot));
            UoW.SaveChanges();
        }

        public void ChangeLot(int lotId, Lot newLot)
        {
            if (UoW.Lots.Get(lotId) == null)
                throw new ArgumentException("Lot id is incorrect");
            mapper.Map<Lot, LotEntity>(newLot, UoW.Lots.Get(lotId));
            UoW.SaveChanges();
            //UoW.Lots.Modify(lotId, mapper.Map<LotEntity>(newLot));
            //UoW.Lots.Delete(lotId);
            //UoW.SaveChanges();
            //UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
            //UoW.SaveChanges();
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
            return GetAllLots(includeProperties).AsQueryable().Where(predicate).AsQueryable();
        }

        public void PlaceBet(int buyerUserId, int lotId, double price)
        {
            if (UoW.UserAccounts.Get(buyerUserId) == null)
                throw new ArgumentException("User id is incorrect");
            if (UoW.Lots.Get(lotId) == null)
                throw new ArgumentException("Lot id is incorrect");
            LotEntity lotModel = UoW.Lots.Get(lotId, lot => lot.LotPhotos, lot => lot.Comments, lot => lot.SellerUser/*,lot => lot.BuyerUser*/);
            lotModel.BuyerUserId = buyerUserId;
            lotModel.Price = price;

            UoW.Lots.Modify(lotId, mapper.Map<LotEntity>(lotModel));

            //mapper.Map<LotEntity, LotEntity>(lotModel, UoW.Lots.Get(lotModel.Id));
            //UoW.Lots.Delete(lotModel.Id);
            //UoW.Lots.Add(mapper.Map<LotEntity>(lotModel));
            UoW.SaveChanges();
        }
    }
}
