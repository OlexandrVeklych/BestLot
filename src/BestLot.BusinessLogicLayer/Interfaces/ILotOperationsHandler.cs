using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BestLot.BusinessLogicLayer.Models;

namespace BestLot.BusinessLogicLayer.Interfaces
{
    public interface ILotOperationsHandler : IDisposable
    {
        //For dependency injection into property
        ILotPhotoOperationsHandler LotPhotoOperationsHandler { get; set; }
        void AddLot(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart);
        Task AddLotAsync(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart);
        void ChangeLot(int lotId, Lot newLot);
        Task ChangeLotAsync(int lotId, Lot newLot);
        void ChangeLotUnsafe(Lot newLot);
        Task ChangeLotUnsafeAsync(Lot newLot);
        void DeleteLot(int lotId, string hostingEnvironmentPath, string requestUriLeftPart);
        Task DeleteLotAsync(int lotId, string hostingEnvironmentPath, string requestUriLeftPart);
        Lot GetLot(int lotId);
        Task<Lot> GetLotAsync(int lotId);
        IQueryable<Lot> GetAllLots();
        Task<IQueryable<Lot>> GetAllLotsAsync();
        IQueryable<Lot> GetUserLots(string userEmail);
        Task<IQueryable<Lot>> GetUserLotsAsync(string userEmail);
        void PlaceBid(int lotId, string buyerUserEmail, double price);
        Task PlaceBidAsync(int lotId, string buyerUserEmail, double price);
        Task<(double Price, DateTime StartDate, DateTime SellDate)> GetBidInfoAsync(int lotId);
        (double Price, DateTime StartDate, DateTime SellDate) GetBidInfo(int lotId);
        void RefreshDBs();
    }
}
