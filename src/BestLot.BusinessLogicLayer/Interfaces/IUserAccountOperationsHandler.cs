using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BestLot.BusinessLogicLayer.Models;

namespace BestLot.BusinessLogicLayer.Interfaces
{
    public interface IUserAccountOperationsHandler : IDisposable
    {
        void AddUserAccount(UserAccountInfo userAccount);
        Task AddUserAccountAsync(UserAccountInfo userAccount);
        void ChangeUserAccount(string userEmail, UserAccountInfo newUserAccount);
        Task ChangeUserAccountAsync(string userEmail, UserAccountInfo newUserAccount);
        void DeleteUserAccount(string userEmail, string hostingEnvironmentPath, string requestUriLeftPart);
        Task DeleteUserAccountAsync(string userEmail, string hostingEnvironmentPath, string requestUriLeftPart);
        UserAccountInfo GetUserAccount(string userEmail);
        Task<UserAccountInfo> GetUserAccountAsync(string userEmail);
        UserAccountInfo GetSellerUser(int lotId);
        Task<UserAccountInfo> GetSellerUserAsync(int lotId);
        UserAccountInfo GetBuyerUser(int lotId);
        Task<UserAccountInfo> GetBuyerUserAsync(int lotId);
        IQueryable<UserAccountInfo> GetAllUserAccounts();
        Task<IQueryable<UserAccountInfo>> GetAllUserAccountsAsync();
    }
}
