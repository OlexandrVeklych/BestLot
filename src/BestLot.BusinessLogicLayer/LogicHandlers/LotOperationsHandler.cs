using System;
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
using BestLot.BusinessLogicLayer.Interfaces;

namespace BestLot.BusinessLogicLayer.LogicHandlers
{
    public class LotOperationsHandler : ILotOperationsHandler
    {
        private LotOperationsHandler()
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
                cfg.CreateMap<LotPhotoEntity, LotPhoto>().MaxDepth(1); ;
                cfg.CreateMap<LotPhoto, LotPhotoEntity>();
                cfg.CreateMap<UserAccountInfo, UserAccountInfoEntity>();
                cfg.CreateMap<UserAccountInfoEntity, UserAccountInfo>().MaxDepth(1); ;
                cfg.CreateMap<Expression<Func<Lot, object>>[], Expression<Func<LotEntity, object>>[]>();
            }).CreateMapper();
        }

        public LotOperationsHandler(IUnitOfWork unitOfWork, ILotPhotoOperationsHandler lotPhotoOperationsHandler) : this()
        {
            this.UoW = unitOfWork;
            this.lotPhotoOperationsHandler = lotPhotoOperationsHandler;
        }

        private ILotPhotoOperationsHandler lotPhotoOperationsHandler;
        public ILotPhotoOperationsHandler LotPhotoOperationsHandler
        {
            get
            {
                return lotPhotoOperationsHandler;
            }
            set
            {
                lotPhotoOperationsHandler = value;
            }
        }
        private IUnitOfWork UoW;
        private IMapper mapper;

        public void AddLot(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            if (UoW.UserAccounts.Get(lot.SellerUserId) == null)
                throw new ArgumentException("Seller user id is incorrect");
            if (lot.StartDate == null || lot.StartDate.Year < 2018)
                lot.StartDate = DateTime.Now;
            lot.BuyerUserId = null;
            if (lot.LotPhotos != null && lot.LotPhotos.Any())
                lotPhotoOperationsHandler.AddPhotosToNewLot(lot, hostingEnvironmentPath, requestUriLeftPart);
            UoW.Lots.Add(mapper.Map<LotEntity>(lot));
            UoW.SaveChanges();
        }

        public async Task AddLotAsync(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            if (await UoW.UserAccounts.GetAsync(lot.SellerUserId) == null)
                throw new ArgumentException("Seller user id is incorrect");
            if (lot.StartDate == null || lot.StartDate.Year < 2018)
                lot.StartDate = DateTime.Now;
            lot.BuyerUserId = null;
            if (lot.LotPhotos != null && lot.LotPhotos.Any())
                lotPhotoOperationsHandler.AddPhotosToNewLot(lot, hostingEnvironmentPath, requestUriLeftPart);
            UoW.Lots.Add(mapper.Map<LotEntity>(lot));
            await UoW.SaveChangesAsync();
        }

        public void ChangeLot(int id, Lot newLot, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            if (UoW.Lots.Get(id) == null)
                AddLot(newLot, hostingEnvironmentPath, requestUriLeftPart);
            else
            {
                Lot currentLot = mapper.Map<Lot>(UoW.Lots.Get(id));
                if (currentLot.Id != newLot.Id
                    || currentLot.SellerUserId != newLot.SellerUserId
                    || currentLot.BuyerUserId != newLot.BuyerUserId
                    || (currentLot.BuyerUserId != null && 
                    (currentLot.Price != newLot.Price
                    || currentLot.StartDate != newLot.StartDate
                    || currentLot.SellDate != newLot.SellDate
                    || currentLot.MinStep != newLot.MinStep))
                    || (currentLot.StartDate.CompareTo(DateTime.Now) >= 0 && currentLot.StartDate != newLot.StartDate))
                    throw new ArgumentException("No permission to change these properties");
                UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
                UoW.SaveChanges();
            }
        }

        public async Task ChangeLotAsync(int id, Lot newLot, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            if (await UoW.Lots.GetAsync(id) == null)
                await AddLotAsync(newLot, hostingEnvironmentPath, requestUriLeftPart);
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
        public void ChangeLotUnsafe(Lot newLot)
        {
            UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
            UoW.SaveChanges();
        }

        //id of newLot is correct, don`t check it again
        public async Task ChangeLotUnsafeAsync(Lot newLot)
        {
            UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
            await UoW.SaveChangesAsync();
        }

        public void DeleteLot(int lotId, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            Lot lot;
            if ((lot = mapper.Map<Lot>(UoW.Lots.Get(lotId))) == null)
                throw new ArgumentException("Lot id is incorrect");
            lotPhotoOperationsHandler.DeleteAllLotPhotos(lot.Id, hostingEnvironmentPath, requestUriLeftPart);
            foreach (LotComment lotComment in lot.LotComments)
                UoW.LotComments.Delete(lotComment.Id); UoW.Lots.Delete(lotId);
            UoW.Lots.Delete(lotId);
            UoW.SaveChanges();
        }

        public async Task DeleteLotAsync(int lotId, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            Lot lot;
            if ((lot = mapper.Map<Lot>(await UoW.Lots.GetAsync(lotId))) == null)
                throw new ArgumentException("Lot id is incorrect");
            await lotPhotoOperationsHandler.DeleteAllLotPhotosAsync(lot.Id, hostingEnvironmentPath, requestUriLeftPart);
            foreach (LotComment lotComment in lot.LotComments)
                UoW.LotComments.Delete(lotComment.Id);
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

        public IQueryable<Lot> GetUserLots(string userId, params Expression<Func<Lot, object>>[] includeProperties)
        {
            Expression<Func<LotEntity, bool>> predicate = null;
            return UoW.Lots.GetAll()
                .Where(predicate = lot => lot.SellerUserId == userId)
                .ProjectTo<Lot>(mapper.ConfigurationProvider);
        }

        public async Task<IQueryable<Lot>> GetUserLotsAsync(string userId, params Expression<Func<Lot, object>>[] includeProperties)
        {
            Expression<Func<LotEntity, bool>> predicate = null;
            var result = await UoW.Lots.GetAllAsync();
            return (await UoW.Lots.GetAllAsync())
                .Where(predicate = lot => lot.SellerUserId == userId)
                .ProjectTo<Lot>(mapper.ConfigurationProvider);
        }

        public void PlaceBid(int lotId, string buyerUserId, double price)
        {
            if (UoW.UserAccounts.Get(buyerUserId) == null)
                throw new ArgumentException("User id is incorrect");
            Lot lot = GetLot(lotId);
            if (lot.SellerUserId == buyerUserId)
                throw new ArgumentException("Placing bids for own lots is not allowed");

            lot.PlaceBid(buyerUserId, price);

            //private ChangeLot, without checking LotId
            ChangeLotUnsafe(lot);
        }

        public async Task PlaceBidAsync(int lotId, string buyerUserId, double price)
        {
            if (await UoW.UserAccounts.GetAsync(buyerUserId) == null)
                throw new ArgumentException("User id is incorrect");
            Lot lot = await GetLotAsync(lotId);
            if (lot.SellerUserId == buyerUserId)
                throw new ArgumentException("Placing bids for own lots is not allowed");

            lot.PlaceBid(buyerUserId, price);

            //private ChangeLot, without checking LotId
            await ChangeLotUnsafeAsync(lot);
        }

        public double GetLotPrice(int lotId)
        {
            return GetLot(lotId).Price;
        }

        public async Task<double> GetLotPriceAsync(int lotId)
        {
            return (await GetLotAsync(lotId)).Price;
        }

        public DateTime GetLotSellDate(int lotId)
        {
            return GetLot(lotId).SellDate;
        }

        public async Task<DateTime> GetLotSellDateAsync(int lotId)
        {
            return (await GetLotAsync(lotId)).SellDate;
        }

        public DateTime GetLotStartDate(int lotId)
        {
            return GetLot(lotId).StartDate;
        }

        public async Task<DateTime> GetLotStartDateAsync(int lotId)
        {
            return (await GetLotAsync(lotId)).StartDate;
        }
    }
}
