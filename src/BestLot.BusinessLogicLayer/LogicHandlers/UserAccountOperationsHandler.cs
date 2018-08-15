using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BestLot.DataAccessLayer.UnitOfWork;
using BestLot.DataAccessLayer.Entities;
using BestLot.BusinessLogicLayer.Models;
using BestLot.BusinessLogicLayer.Exceptions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using System.Net.Mail;
using BestLot.BusinessLogicLayer.Interfaces;

namespace BestLot.BusinessLogicLayer.LogicHandlers
{
    public class UserAccountOperationsHandler : IUserAccountOperationsHandler
    {
        private UserAccountOperationsHandler()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LotEntity, Lot>()
                .ForAllMembers(opt => opt.Ignore());
                cfg.CreateMap<UserAccountInfoEntity, UserAccountInfo>()
                .ForMember(dest => dest.Lots, opt => opt.Ignore())
                .ForMember(dest => dest.LotComments, opt => opt.Ignore());
                cfg.CreateMap<LotPhotoEntity, LotPhoto>()
                .ForAllMembers(opt => opt.Ignore());
                cfg.CreateMap<LotCommentEntity, LotComment>()
                .ForAllMembers(opt => opt.Ignore());

                cfg.CreateMap<UserAccountInfo, UserAccountInfoEntity>();
            }).CreateMapper();
        }

        public UserAccountOperationsHandler(IUnitOfWork unitOfWork, ILotOperationsHandler lotOperationsHandler, ILotPhotoOperationsHandler lotPhotoOperationsHandler) : this()
        {
            this.UoW = unitOfWork;
            this.lotOperationsHandler = lotOperationsHandler;
            this.lotPhotoOperationsHandler = lotPhotoOperationsHandler;
        }

        private ILotPhotoOperationsHandler lotPhotoOperationsHandler;
        private ILotOperationsHandler lotOperationsHandler;
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

        public void ChangeUserAccount(string userEmail, UserAccountInfo newUserAccount)
        {
            UserAccountInfo currentUser = GetUserAccount(userEmail);
            if (currentUser.Email != newUserAccount.Email)
                throw new WrongModelException("No permission to change Email");
            ValidateUser(newUserAccount, currentUser, false);
            UoW.UserAccounts.Modify(userEmail, mapper.Map<UserAccountInfoEntity>(newUserAccount));
            UoW.SaveChanges();
        }

        public async Task ChangeUserAccountAsync(string userEmail, UserAccountInfo newUserAccount)
        {
            UserAccountInfo currentUser = await GetUserAccountAsync(userEmail);
            if (currentUser.Email != newUserAccount.Email)
                throw new WrongModelException("No permission to change Email");
            await ValidateUserAsync(newUserAccount, currentUser, false);
            UoW.UserAccounts.Modify(userEmail, mapper.Map<UserAccountInfoEntity>(newUserAccount));
            await UoW.SaveChangesAsync();
        }

        private void ValidateUser(UserAccountInfo userAccount, UserAccountInfo oldUserAccount = null, bool newUser = true)
        {
            if (newUser)
            {
                try
                {
                    MailAddress temp = new MailAddress(userAccount.Email);
                }
                catch (FormatException)
                {
                    throw new WrongModelException("Incorrect email format");
                }
                var userAccountsEmails = GetAllUserAccounts().Select(user => user.Email);
                if (userAccountsEmails.Contains(userAccount.Email))
                    throw new WrongModelException("Email is already occupied");
            }
            //if !newUser, don`t check email because it can`t be changed
            if (userAccount.TelephoneNumber != null)
            {
                //If user isn`t new and didn`t change telephone number, don`t check it
                if (oldUserAccount != null && oldUserAccount.TelephoneNumber != null  && userAccount.TelephoneNumber == oldUserAccount.TelephoneNumber)
                    return;
                if (!Regex.IsMatch(userAccount.TelephoneNumber, @"^\+380[0-9]{9}$"))
                    throw new WrongModelException("Incorrect telephone number format");
                var userAccountsTelephones = GetAllUserAccounts().Select(user => user.TelephoneNumber);
                if (userAccountsTelephones.Contains(userAccount.TelephoneNumber))
                    throw new WrongModelException("Telephone number is already occupied");
            }
            //No exception - userAccount is correct
        }

        private async Task ValidateUserAsync(UserAccountInfo userAccount, UserAccountInfo oldUserAccount = null, bool newUser = true)
        {
            if (newUser)
            {
                try
                {
                    MailAddress temp = new MailAddress(userAccount.Email);
                }
                catch (FormatException)
                {
                    throw new WrongModelException("Incorrect email format");
                }
                var userAccountsEmails = (await GetAllUserAccountsAsync()).Select(user => user.Email);
                if (userAccountsEmails.Contains(userAccount.Email))
                    throw new WrongModelException("Email is already occupied");
            }
            //if !newUser, don`t check email because it can`t be changed
            if (userAccount.TelephoneNumber != null)
            {
                //If user isn`t new and didn`t change telephone number, don`t check it
                if (oldUserAccount != null && userAccount.TelephoneNumber == oldUserAccount.TelephoneNumber)
                    return;
                if (!Regex.IsMatch(userAccount.TelephoneNumber, @"^\+380[0-9]{9}$"))
                    throw new WrongModelException("Incorrect telephone number format");
                var userAccountsTelephones = GetAllUserAccounts().Select(user => user.TelephoneNumber);
                if (userAccountsTelephones.Contains(userAccount.TelephoneNumber))
                    throw new WrongModelException("Telephone number is already occupied");
            }
            //No exception - userAccount is correct
        }

        public void DeleteUserAccount(string userEmail, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            UserAccountInfo user = GetUserAccount(userEmail);
            lotPhotoOperationsHandler.DeleteAllUserPhotos(userEmail, hostingEnvironmentPath);
            UoW.UserAccounts.Delete(userEmail);
            UoW.SaveChanges();
        }

        public async Task DeleteUserAccountAsync(string userEmail, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            UserAccountInfo user = await GetUserAccountAsync(userEmail);
            await lotPhotoOperationsHandler.DeleteAllUserPhotosAsync(userEmail, hostingEnvironmentPath);
            UoW.UserAccounts.Delete(userEmail);
            await UoW.SaveChangesAsync();
        }

        public UserAccountInfo GetSellerUser(int lotId)
        {
            Lot lot = lotOperationsHandler.GetLot(lotId);
            return GetUserAccount(lot.SellerUserId);
        }

        public async Task<UserAccountInfo> GetSellerUserAsync(int lotId)
        {
            Lot lot = await lotOperationsHandler.GetLotAsync(lotId);
            return await GetUserAccountAsync(lot.SellerUserId);
        }

        public UserAccountInfo GetBuyerUser(int lotId)
        {
            Lot lot = lotOperationsHandler.GetLot(lotId);
            try
            {
                return GetUserAccount(lot.BuyerUserId);
            }
            catch (WrongIdException)
            {
                return null;
            }
        }

        public async Task<UserAccountInfo> GetBuyerUserAsync(int lotId)
        {
            Lot lot = await lotOperationsHandler.GetLotAsync(lotId);
            try
            {
                return await GetUserAccountAsync(lot.BuyerUserId);
            }
            catch (WrongIdException)
            {
                return null;
            }
        }

        public UserAccountInfo GetUserAccount(string userEmail)
        {
            UserAccountInfoEntity userAccountInfo = UoW.UserAccounts.Get(userEmail) ?? throw new WrongIdException("UserAccount");
            return mapper.Map<UserAccountInfo>(userAccountInfo);
        }

        public async Task<UserAccountInfo> GetUserAccountAsync(string userEmail)
        {
            UserAccountInfoEntity userAccountInfo = await UoW.UserAccounts.GetAsync(userEmail) ?? throw new WrongIdException("UserAccount");
            return mapper.Map<UserAccountInfo>(userAccountInfo);
        }

        public IQueryable<UserAccountInfo> GetAllUserAccounts()
        {
            return UoW.UserAccounts.GetAll().ProjectTo<UserAccountInfo>(mapper.ConfigurationProvider);
        }

        public async Task<IQueryable<UserAccountInfo>> GetAllUserAccountsAsync()
        {
            return (await UoW.UserAccounts.GetAllAsync()).ProjectTo<UserAccountInfo>(mapper.ConfigurationProvider);
        }
    }
}
