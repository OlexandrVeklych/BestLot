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
        void PlaceBet(string buyerUserId, int lotId, double price);
        Task PlaceBetAsync(string buyerUserId, int lotId, double price);
    }
}
