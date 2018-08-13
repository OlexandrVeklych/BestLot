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
        void DeleteUserAccount(string userAccountId, string hostingEnvironmentPath, string requestUriLeftPart);
        Task DeleteUserAccountAsync(string userAccountId, string hostingEnvironmentPath, string requestUriLeftPart);
        UserAccountInfo GetUserAccount(string userAccountId);
        Task<UserAccountInfo> GetUserAccountAsync(string userAccountId);
        UserAccountInfo GetSellerUser(int lotId);
        Task<UserAccountInfo> GetSellerUserAsync(int lotId);
        UserAccountInfo GetBuyerUser(int lotId);
        Task<UserAccountInfo> GetBuyerUserAsync(int lotId);
        IQueryable<UserAccountInfo> GetAllUserAccounts();
        Task<IQueryable<UserAccountInfo>> GetAllUserAccountsAsync();
    }
}
