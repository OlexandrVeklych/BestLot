using BestLot.BusinessLogicLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.Interfaces
{
    public interface ILotCommentOperationsHandler : IDisposable
    {
        void AddComment(LotComment lotComment);
        Task AddCommentAsync(LotComment lotComment);
        IQueryable<LotComment> GetLotComments(int lotId);
        Task<IQueryable<LotComment>> GetLotCommentsAsync(int lotId);
        IQueryable<LotComment> GetUserComments(string userEmail);
        Task<IQueryable<LotComment>> GetUserCommentsAsync(string userEmail);
    }
}
