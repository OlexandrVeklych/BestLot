using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BestLot.BusinessLogicLayer.Models;

namespace BestLot.BusinessLogicLayer.Interfaces
{
    public interface ILotOperationsHandler
    {
        ILotPhotoOperationsHandler LotPhotoOperationsHandler { get; set; }
        void AddLot(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart);
        Task AddLotAsync(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart);
        void ChangeLot(int id, Lot newLot, string hostingEnvironmentPath, string requestUriLeftPart);
        Task ChangeLotAsync(int id, Lot newLot, string hostingEnvironmentPath, string requestUriLeftPart);
        void ChangeLotUnsafe(Lot newLot);
        Task ChangeLotUnsafeAsync(Lot newLot);
        void DeleteLot(int lotId, string hostingEnvironmentPath, string requestUriLeftPart);
        Task DeleteLotAsync(int lotId, string hostingEnvironmentPath, string requestUriLeftPart);
        Lot GetLot(int lotId, params Expression<Func<Lot, object>>[] includeProperties);
        Task<Lot> GetLotAsync(int lotId, params Expression<Func<Lot, object>>[] includeProperties);
        IQueryable<Lot> GetAllLots(params Expression<Func<Lot, object>>[] includeProperties);
        Task<IQueryable<Lot>> GetAllLotsAsync(params Expression<Func<Lot, object>>[] includeProperties);
        IQueryable<Lot> GetUserLots(string userId, params Expression<Func<Lot, object>>[] includeProperties);
        Task<IQueryable<Lot>> GetUserLotsAsync(string userId, params Expression<Func<Lot, object>>[] includeProperties);
        void PlaceBid(int lotId, string buyerUserId, double price);
        Task PlaceBidAsync(int lotId, string buyerUserId, double price);
        double GetLotPrice(int lotId);
        Task<double> GetLotPriceAsync(int lotId);
        DateTime GetLotSellDate(int lotId);
        Task<DateTime> GetLotSellDateAsync(int lotId);
        DateTime GetLotStartDate(int lotId);
        Task<DateTime> GetLotStartDateAsync(int lotId);
    }
}
