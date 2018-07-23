using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BestLot.BusinessLogicLayer.Models;

namespace BestLot.BusinessLogicLayer.LogicHandlers
{
    public interface IUserAccountOperationsHandler
    {
        void AddUserAccount(UserAccountInfo userAccount);
        void ChangeUserAccount(string id, UserAccountInfo newUserAccount);
        void DeleteUserAccount(string userAccountId);
        UserAccountInfo GetUserAccount(string userAccountId);
        UserAccountInfo GetUserAccount(string userAccountId, params Expression<Func<UserAccountInfo, object>>[] includeProperties);
        IQueryable<UserAccountInfo> GetAllUserAccounts();
        IQueryable<UserAccountInfo> GetAllUserAccounts(params Expression<Func<UserAccountInfo, object>>[] includeProperties);
    }
}
