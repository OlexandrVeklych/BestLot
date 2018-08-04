using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BestLot.BusinessLogicLayer.Models;

namespace BestLot.BusinessLogicLayer.Interfaces
{
    public interface IUserAccountOperationsHandler
    {
        void AddUserAccount(UserAccountInfo userAccount);
        Task AddUserAccountAsync(UserAccountInfo userAccount);
        void ChangeUserAccount(string id, UserAccountInfo newUserAccount);
        Task ChangeUserAccountAsync(string id, UserAccountInfo newUserAccount);
        void DeleteUserAccount(string userAccountId, string hostingEnvironmentPath);
        Task DeleteUserAccountAsync(string userAccountId, string hostingEnvironmentPath);
        UserAccountInfo GetUserAccount(string userAccountId, params Expression<Func<UserAccountInfo, object>>[] includeProperties);
        Task<UserAccountInfo> GetUserAccountAsync(string userAccountId, params Expression<Func<UserAccountInfo, object>>[] includeProperties);
        UserAccountInfo GetSellerUser(int lotId, params Expression<Func<UserAccountInfo, object>>[] includeProperties);
        Task<UserAccountInfo> GetSellerUserAsync(int lotId, params Expression<Func<UserAccountInfo, object>>[] includeProperties);
        UserAccountInfo GetBuyerUser(int lotId, params Expression<Func<UserAccountInfo, object>>[] includeProperties);
        Task<UserAccountInfo> GetBuyerUserAsync(int lotId, params Expression<Func<UserAccountInfo, object>>[] includeProperties);
        IQueryable<UserAccountInfo> GetAllUserAccounts(params Expression<Func<UserAccountInfo, object>>[] includeProperties);
        Task<IQueryable<UserAccountInfo>> GetAllUserAccountsAsync(params Expression<Func<UserAccountInfo, object>>[] includeProperties);
    }
}
