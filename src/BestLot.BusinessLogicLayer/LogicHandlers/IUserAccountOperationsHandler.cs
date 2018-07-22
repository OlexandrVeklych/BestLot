using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessLogicLayer.Models;

namespace BusinessLogicLayer.LogicHandlers
{
    public interface IUserAccountOperationsHandler
    {
        void AddUserAccount(UserAccountInfo userAccount);
        void ChangeUserAccount(int id, UserAccountInfo newUserAccount);
        void DeleteUserAccount(int userAccountId);
        UserAccountInfo GetUserAccount(int userAccountId);
        UserAccountInfo GetUserAccount(int userAccountId, params Expression<Func<UserAccountInfo, object>>[] includeProperties);
        IQueryable<UserAccountInfo> GetAllUserAccounts();
        IQueryable<UserAccountInfo> GetAllUserAccounts(params Expression<Func<UserAccountInfo, object>>[] includeProperties);
    }
}
