using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestLot.DataAccessLayer.UnitOfWork;
using BestLot.DataAccessLayer.Entities;
using BestLot.BusinessLogicLayer.Models;
using BestLot.BusinessLogicLayer.Exceptions;
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
                cfg.CreateMap<LotEntity, Lot>()
                .ForMember(dest => dest.LotComments, opt => opt.Ignore())
                .ForMember(dest => dest.LotPhotos, opt => opt.Ignore())
                .ForMember(dest => dest.SellerUser, opt => opt.Ignore());
                cfg.CreateMap<UserAccountInfoEntity, UserAccountInfo>()
                .ForAllMembers(opt => opt.Ignore());
                cfg.CreateMap<LotPhotoEntity, LotPhoto>()
                .ForAllMembers(opt => opt.Ignore());
                cfg.CreateMap<LotCommentEntity, LotComment>()
                .ForAllMembers(opt => opt.Ignore());

                cfg.CreateMap<Lot, LotEntity>();
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

        //hostingEnvironmentPath - physical path to WebAPI folder
        //requestUriLeftPart - URL
        public void AddLot(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            if (lot.StartDate == null || lot.StartDate.CompareTo(DateTime.Now.AddSeconds(-10)) < 0)
                throw new WrongModelException("Wrong start date");
            if (lot.StartDate.CompareTo(lot.SellDate) >= 0)
                throw new WrongModelException("Wrong sell date");
            if (UoW.UserAccounts.Get(lot.SellerUserId) == null)
                throw new WrongIdException("Seller user");
            lot.BuyerUserId = null;
            if (lot.LotPhotos != null && lot.LotPhotos.Any())
                lotPhotoOperationsHandler.AddPhotosToNewLot(lot, hostingEnvironmentPath, requestUriLeftPart);
            UoW.Lots.Add(mapper.Map<LotEntity>(lot));
            UoW.SaveChanges();
        }

        //hostingEnvironmentPath - physical path to WebAPI folder
        //requestUriLeftPart - URL
        public async Task AddLotAsync(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            if (lot.StartDate == null || lot.StartDate.CompareTo(DateTime.Now.AddSeconds(-10)) < 0)
                throw new WrongModelException("Wrong start date");
            if (lot.StartDate.CompareTo(lot.SellDate) >= 0)
                throw new WrongModelException("Wrong sell date");
            if (await UoW.UserAccounts.GetAsync(lot.SellerUserId) == null)
                throw new WrongIdException("Seller user");
            if (lot.BidPlacer == "Relative")
            {
                lot.SellDate = lot.StartDate.Add(lot.SellDate.Subtract(lot.StartDate));
            }
            lot.BuyerUserId = null;
            if (lot.LotPhotos != null && lot.LotPhotos.Any())
                lotPhotoOperationsHandler.AddPhotosToNewLot(lot, hostingEnvironmentPath, requestUriLeftPart);
            UoW.Lots.Add(mapper.Map<LotEntity>(lot));
            await UoW.SaveChangesAsync();
        }

        public void ChangeLot(int lotId, Lot newLot)
        {
            Lot currentLot = GetLot(lotId);
            if (currentLot.Id != newLot.Id //Changed id
                || currentLot.SellerUserId != newLot.SellerUserId//Changed seller user
                || currentLot.BuyerUserId != newLot.BuyerUserId//Changed buyer user
                || (newLot.StartDate.CompareTo(DateTime.Now.AddSeconds(-10)) < 0) //Start date before now
                || (currentLot.BuyerUserId != null &&
                (currentLot.Price != newLot.Price //Lot has buyer (at least 1 bid was placed) and Price changed
                || currentLot.StartDate != newLot.StartDate //Lot has buyer (at least 1 bid was placed) and start date changed
                || currentLot.SellDate != newLot.SellDate //Lot has buyer (at least 1 bid was placed) and sell date changed
                || currentLot.Currency != newLot.Currency //Lot has buyer (at least 1 bid was placed) and currency changed
                || currentLot.MinStep != newLot.MinStep))) //Lot has buyer (at least 1 bid was placed) and step bid changed
                throw new WrongModelException("No permission to change these properties");
            UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
            UoW.SaveChanges();

        }

        public async Task ChangeLotAsync(int lotId, Lot newLot)
        {
            Lot currentLot = mapper.Map<Lot>(await UoW.Lots.GetAsync(lotId));
            if (currentLot.Id != newLot.Id //Changed id
                || currentLot.SellerUserId != newLot.SellerUserId//Changed seller user
                || currentLot.BuyerUserId != newLot.BuyerUserId//Changed buyer user
                || (newLot.StartDate.CompareTo(DateTime.Now.AddSeconds(-10)) < 0) //Start date before now
                || (currentLot.BuyerUserId != null &&
                (currentLot.Price != newLot.Price //Lot has buyer (at least 1 bid was placed) and Price changed
                || currentLot.StartDate != newLot.StartDate //Lot has buyer (at least 1 bid was placed) and start date changed
                || currentLot.SellDate != newLot.SellDate //Lot has buyer (at least 1 bid was placed) and sell date changed
                || currentLot.Currency != newLot.Currency //Lot has buyer (at least 1 bid was placed) and currency changed
                || currentLot.MinStep != newLot.MinStep))) //Lot has buyer (at least 1 bid was placed) and step bid changed
                throw new WrongModelException("No permission to change these properties");
            UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
            await UoW.SaveChangesAsync();
        }

        //Lot is correct, don`t check it again
        public void ChangeLotUnsafe(Lot newLot)
        {
            UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
            UoW.SaveChanges();
        }

        //Lot is correct, don`t check it again
        public async Task ChangeLotUnsafeAsync(Lot newLot)
        {
            UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
            await UoW.SaveChangesAsync();
        }

        //hostingEnvironmentPath - physical path to WebAPI folder
        //requestUriLeftPart - URL
        public void DeleteLot(int lotId, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            Lot lot = GetLot(lotId);
            //Don`t use cascade deleting, because photos must be deleted as well
            lotPhotoOperationsHandler.DeleteAllLotPhotos(lot.Id, hostingEnvironmentPath, requestUriLeftPart);
            UoW.Lots.Delete(lotId);
            UoW.SaveChanges();
        }

        //hostingEnvironmentPath - physical path to WebAPI folder
        //requestUriLeftPart - URL
        public async Task DeleteLotAsync(int lotId, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            Lot lot = await GetLotAsync(lotId);
            //Don`t use cascade deleting, because photos must be deleted as well
            await lotPhotoOperationsHandler.DeleteAllLotPhotosAsync(lot.Id, hostingEnvironmentPath, requestUriLeftPart);
            UoW.Lots.Delete(lotId);
            await UoW.SaveChangesAsync();
        }

        public Lot GetLot(int lotId)
        {
            LotEntity lot = UoW.Lots.Get(lotId) ?? throw new WrongIdException("Lot");
            return mapper.Map<Lot>(lot);
        }

        public async Task<Lot> GetLotAsync(int lotId)
        {
            LotEntity lot = await UoW.Lots.GetAsync(lotId) ?? throw new WrongIdException("Lot");
            return mapper.Map<Lot>(lot);
        }

        public IQueryable<Lot> GetAllLots()
        {
            return UoW.Lots.GetAll().ProjectTo<Lot>(mapper.ConfigurationProvider);
        }

        public async Task<IQueryable<Lot>> GetAllLotsAsync()
        {
            return (await UoW.Lots.GetAllAsync()).ProjectTo<Lot>(mapper.ConfigurationProvider);
        }

        public IQueryable<Lot> GetUserLots(string userEmail)
        {
            Expression<Func<LotEntity, bool>> predicate = null;
            return UoW.Lots.GetAll()
                .Where(predicate = lot => lot.SellerUserId == userEmail)
                .ProjectTo<Lot>(mapper.ConfigurationProvider);
        }

        public async Task<IQueryable<Lot>> GetUserLotsAsync(string userEmail)
        {
            Expression<Func<LotEntity, bool>> predicate = null;
            var result = await UoW.Lots.GetAllAsync();
            return (await UoW.Lots.GetAllAsync())
                .Where(predicate = lot => lot.SellerUserId == userEmail)
                .ProjectTo<Lot>(mapper.ConfigurationProvider);
        }

        public void PlaceBid(int lotId, string buyerUserEmail, double price)
        {
            Lot lot = GetLot(lotId);
            if (lot.SellerUserId == buyerUserEmail)
                throw new WrongModelException("Placing bids for own lots is not allowed");
            if (UoW.UserAccounts.Get(buyerUserEmail) == null)
                throw new WrongIdException("User");
            lot.PlaceBid(buyerUserEmail, price);

            //private ChangeLot, without checking Lot
            ChangeLotUnsafe(lot);
        }

        public async Task PlaceBidAsync(int lotId, string buyerUserEmail, double price)
        {
            Lot lot = await GetLotAsync(lotId);
            if (lot.SellerUserId == buyerUserEmail)
                throw new WrongModelException("Placing bids for own lots is not allowed");
            if (await UoW.UserAccounts.GetAsync(buyerUserEmail) == null)
                throw new WrongIdException("User");

            lot.PlaceBid(buyerUserEmail, price);

            //private ChangeLot, without checking Lot
            await ChangeLotUnsafeAsync(lot);
        }

        public async Task<(double Price, DateTime StartDate, DateTime SellDate)> GetBidInfoAsync(int lotId)
        {
            Lot lot = await GetLotAsync(lotId);
            return (lot.Price, lot.StartDate, lot.SellDate);
        }

        public (double Price, DateTime StartDate, DateTime SellDate) GetBidInfo(int lotId)
        {
            Lot lot = GetLot(lotId);
            return (lot.Price, lot.StartDate, lot.SellDate);
        }

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                UoW.Dispose();
                lotPhotoOperationsHandler.Dispose();
                if (disposing)
                {
                    lotPhotoOperationsHandler = null;
                    mapper = null;
                    UoW = null;
                }
            }
            disposed = true;
        }
    }
}
