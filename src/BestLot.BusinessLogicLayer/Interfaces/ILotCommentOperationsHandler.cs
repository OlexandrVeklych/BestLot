using BestLot.BusinessLogicLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.Interfaces
{
    public interface ILotCommentOperationsHandler
    {
        void AddComment(LotComment lotComment);
        Task AddCommentAsync(LotComment lotComment);
        IQueryable<LotComment> GetLotComments(int lotId, params Expression<Func<Lot, object>>[] includeProperties);
        Task<IQueryable<LotComment>> GetLotCommentsAsync(int lotId, params Expression<Func<Lot, object>>[] includeProperties);
        IQueryable<LotComment> GetUserComments(string userId, params Expression<Func<Lot, object>>[] includeProperties);
        Task<IQueryable<LotComment>> GetUserCommentsAsync(string userId, params Expression<Func<Lot, object>>[] includeProperties);
    }
}
