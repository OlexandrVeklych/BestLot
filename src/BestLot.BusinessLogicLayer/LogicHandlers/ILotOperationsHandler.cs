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
        void ChangeLot(int id, Lot newLot);
        void DeleteLot(int lotId);
        Lot GetLot(int lotId);
        Lot GetLot(int lotId, params Expression<Func<Lot, object>>[] includeProperties);
        IQueryable<Lot> GetAllLots();
        IQueryable<Lot> GetAllLots(params Expression<Func<Lot, object>>[] includeProperties);
        void PlaceBet(int buyerUserId, int lotId, double price);
        void AddComment(LotComment lotComment);
    }
}
