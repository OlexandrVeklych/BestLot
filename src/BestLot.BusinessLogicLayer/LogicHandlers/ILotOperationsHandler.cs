using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BestLot.BusinessLogicLayer.Models;

namespace BestLot.BusinessLogicLayer.LogicHandlers
{
    public interface ILotOperationsHandler
    {
        void AddLot(Lot lot);
        Task AddLotAsync(Lot lot);
        void ChangeLot(int id, Lot newLot);
        Task ChangeLotAsync(int id, Lot newLot);
        void DeleteLot(int lotId);
        Task DeleteLotAsync(int lotId);
        Lot GetLot(int lotId, params Expression<Func<Lot, object>>[] includeProperties);
        Task<Lot> GetLotAsync(int lotId, params Expression<Func<Lot, object>>[] includeProperties);
        IQueryable<Lot> GetAllLots(params Expression<Func<Lot, object>>[] includeProperties);
        Task<IQueryable<Lot>> GetAllLotsAsync(params Expression<Func<Lot, object>>[] includeProperties);
        void PlaceBet(string buyerUserId, int lotId, double price);
        Task PlaceBetAsync(string buyerUserId, int lotId, double price);
        void AddComment(LotComment lotComment);
        Task AddCommentAsync(LotComment lotComment);
    }
}
