using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BestLot.DataAccessLayer.UnitOfWork;
using BestLot.DataAccessLayer.Entities;
using BestLot.BusinessLogicLayer.Models;
using AutoMapper;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using System.Net.Mail;

namespace BestLot.BusinessLogicLayer.LogicHandlers
{
    public class UserAccountOperationsHandler : IUserAccountOperationsHandler
    {
        public UserAccountOperationsHandler(IUnitOfWork unitOfWork)
        {
            UoW = unitOfWork;
            mapper = new MapperConfiguration(cfg =>
            {
                //MaxDepth(1) - to map User inside Lot without his Lots
                //Lot.User.Name - ok
                //Lot.User.Lots[n] - null reference
                cfg.CreateMap<LotEntity, Lot>().MaxDepth(1);
                cfg.CreateMap<Lot, LotEntity>();
                //MaxDepth(1) - to map User inside Comment without his Lots
                //LotComment.User.Name - ok
                //LotComment.User.Lots[n] - null reference
                cfg.CreateMap<LotCommentEntity, LotComment>().MaxDepth(1);
                cfg.CreateMap<LotComment, LotCommentEntity>();
                cfg.CreateMap<LotPhotoEntity, LotPhoto>();
                cfg.CreateMap<LotPhoto, LotPhotoEntity>();
                cfg.CreateMap<UserAccountInfo, UserAccountInfoEntity>();
                cfg.CreateMap<UserAccountInfoEntity, UserAccountInfo>();
                cfg.CreateMap<Expression<Func<Lot, object>>[], Expression<Func<LotEntity, object>>[]>();
                cfg.CreateMap<Func<Lot, bool>, Func<LotEntity, bool>>();
                cfg.CreateMap<Func<Lot, object>, Func<LotEntity, object>>();
            }).CreateMapper();
        }

        private IUnitOfWork UoW;
        private IMapper mapper;

        public void AddUserAccount(UserAccountInfo userAccount)
        {
            ValidateUser(userAccount);
            UoW.UserAccounts.Add(mapper.Map<UserAccountInfoEntity>(userAccount));
            UoW.SaveChanges();
        }

        public async Task AddUserAccountAsync(UserAccountInfo userAccount)
        {
            await ValidateUserAsync(userAccount);
            UoW.UserAccounts.Add(mapper.Map<UserAccountInfoEntity>(userAccount));
            await UoW.SaveChangesAsync();
        }

        public void ChangeUserAccount(string id, UserAccountInfo newUserAccount)
        {
            UserAccountInfo currentUser = mapper.Map<UserAccountInfo>(UoW.UserAccounts.Get(id));
            if (currentUser == null)
                throw new ArgumentException("User email is incorrect");
            if (currentUser.Email != newUserAccount.Email)
                throw new ArgumentException("No permission to change Email");
            ValidateUser(newUserAccount, false);
            UoW.UserAccounts.Modify(id, mapper.Map<UserAccountInfoEntity>(newUserAccount));
            UoW.SaveChanges();
        }

        public async Task ChangeUserAccountAsync(string id, UserAccountInfo newUserAccount)
        {
            UserAccountInfo currentUser = mapper.Map<UserAccountInfo>(await UoW.UserAccounts.GetAsync(id));
            if (currentUser == null)
                throw new ArgumentException("User email is incorrect");
            if (currentUser.Email != newUserAccount.Email)
                throw new ArgumentException("No permission to change Email");
            await ValidateUserAsync(newUserAccount, false);
            UoW.UserAccounts.Modify(id, mapper.Map<UserAccountInfoEntity>(newUserAccount));
            await UoW.SaveChangesAsync();
        }

        //if !newUser, don`t check email
        private void ValidateUser(UserAccountInfo userAccount, bool newUser = true)
        {
            if (userAccount.TelephoneNumber != null)
            {
                if (!Regex.IsMatch(userAccount.TelephoneNumber, @"^\+380[0-9]{9}$"))
                    throw new ArgumentException("Incorrect telephone number format");
                var userAccountsTelephones = UoW.UserAccounts.GetAll().Select(user => user.TelephoneNumber);
                if (userAccountsTelephones.Contains(userAccount.TelephoneNumber))
                    throw new ArgumentException("Telephone number is already occupied");
            }
            if (newUser)
            {
                try
                {
                    MailAddress temp = new MailAddress(userAccount.Email);
                }
                catch(FormatException)
                {
                    throw new ArgumentException("Incorrect email format");
                }
                var userAccountsEmails = GetAllUserAccounts().Select(user => user.Email);
                if (userAccountsEmails.Contains(userAccount.Email))
                    throw new ArgumentException("Email is already occupied");
            }
        }

        //if !newUser, don`t check email
        private async Task ValidateUserAsync(UserAccountInfo userAccount, bool newUser = true)
        {
            if (userAccount.TelephoneNumber != null)
            {
                if (!Regex.IsMatch(userAccount.TelephoneNumber, @"^\+380[0-9]{9}$"))
                    throw new ArgumentException("Incorrect telephone number format");
                var userAccountsTelephones = (await GetAllUserAccountsAsync()).Select(user => user.TelephoneNumber);
                if (userAccountsTelephones.Contains(userAccount.TelephoneNumber))
                    throw new ArgumentException("Telephone number is already occupied");
            }
            if (newUser)
            {
                try
                {
                    MailAddress temp = new MailAddress(userAccount.Email);
                }
                catch (FormatException)
                {
                    throw new ArgumentException("Incorrect email format");
                }
                var userAccountsEmails = (await GetAllUserAccountsAsync()).Select(user => user.Email);
                if (userAccountsEmails.Contains(userAccount.Email))
                    throw new ArgumentException("Email is already occupied");
            }
        }

        public void DeleteUserAccount(string userAccountId)
        {
            if (UoW.UserAccounts.Get(userAccountId) == null)
                throw new ArgumentException("UserAccount id is incorrect");
            UoW.UserAccounts.Delete(userAccountId);
            UoW.SaveChanges();
        }

        public async Task DeleteUserAccountAsync(string userAccountId)
        {
            if (await UoW.UserAccounts.GetAsync(userAccountId) == null)
                throw new ArgumentException("UserAccount id is incorrect");
            UoW.UserAccounts.Delete(userAccountId);
            await UoW.SaveChangesAsync();
        }

        public UserAccountInfo GetUserAccount(string userAccountId, params Expression<Func<UserAccountInfo, object>>[] includeProperties)
        {
            UserAccountInfo userAccountInfo = mapper.Map<UserAccountInfo>(UoW.UserAccounts.Get(userAccountId, mapper.Map<Expression<Func<UserAccountInfoEntity, object>>[]>(includeProperties)));
            if (userAccountInfo == null)
                throw new ArgumentException("UserAccount id is incorrect");
            return userAccountInfo;
        }

        public async Task<UserAccountInfo> GetUserAccountAsync(string userAccountId, params Expression<Func<UserAccountInfo, object>>[] includeProperties)
        {
            UserAccountInfo userAccountInfo = mapper.Map<UserAccountInfo>(await UoW.UserAccounts.GetAsync(userAccountId, mapper.Map<Expression<Func<UserAccountInfoEntity, object>>[]>(includeProperties)));
            if (userAccountInfo == null)
                throw new ArgumentException("UserAccount id is incorrect");
            return userAccountInfo;
        }

        public IQueryable<UserAccountInfo> GetAllUserAccounts(params Expression<Func<UserAccountInfo, object>>[] includeProperties)
        {
            return UoW.UserAccounts.GetAll(mapper.Map<Expression<Func<UserAccountInfoEntity, object>>[]>(includeProperties)).ProjectTo<UserAccountInfo>(mapper.ConfigurationProvider);
        }

        public async Task<IQueryable<UserAccountInfo>> GetAllUserAccountsAsync(params Expression<Func<UserAccountInfo, object>>[] includeProperties)
        {
            var result = await UoW.UserAccounts.GetAllAsync(mapper.Map<Expression<Func<UserAccountInfoEntity, object>>[]>(includeProperties));
            return result.ProjectTo<UserAccountInfo>(mapper.ConfigurationProvider);
        }
    }
}
