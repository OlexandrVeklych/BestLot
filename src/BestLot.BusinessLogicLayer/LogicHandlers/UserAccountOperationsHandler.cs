﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.UnitOfWork;
using DataAccessLayer.Entities;
using BusinessLogicLayer.Models;
using AutoMapper;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;

namespace BusinessLogicLayer.LogicHandlers
{
    public class UserAccountOperationsHandler
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
                cfg.CreateMap<UserAccountInfo, UserAccountInfoEntity>().MaxDepth(1);
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
            UoW.UserAccounts.Add(mapper.Map<UserAccountInfoEntity>(userAccount));
            UoW.SaveChanges();
        }

        public void ChangeUserAccount(int userId, UserAccountInfo newUserAccount)
        {
            mapper.Map<UserAccountInfo, UserAccountInfoEntity>(newUserAccount, UoW.UserAccounts.Get(userId));
            UoW.SaveChanges();
        }

        public void DeleteUserAccount(int userAccountId)
        {
            UoW.UserAccounts.Delete(userAccountId);
            UoW.SaveChanges();
        }

        public UserAccountInfo GetUserAccount(int userId)
        {
            if (UoW.UserAccounts.Get(userId) == null)
                throw new ArgumentException("UserAccount id is incorrect");
            return mapper.Map<UserAccountInfo>(UoW.UserAccounts.Get(userId));
        }

        public UserAccountInfo GetUserAccount(int userId, params Expression<Func<UserAccountInfo, object>>[] includeProperties)
        {
            if (UoW.UserAccounts.Get(userId) == null)
                throw new ArgumentException("UserAccount id is incorrect");
            return mapper.Map<UserAccountInfo>(UoW.UserAccounts.Get(userId, mapper.Map<Expression<Func<UserAccountInfoEntity, object>>[]>(includeProperties)));
        }

        public IQueryable<UserAccountInfo> GetAllUserAccounts()
        {
            return UoW.UserAccounts.GetAll().ProjectTo<UserAccountInfo>(mapper.ConfigurationProvider);
        }

        public IQueryable<UserAccountInfo> GetAllUserAccounts(params Expression<Func<UserAccountInfo, object>>[] includeProperties)
        {
            return UoW.UserAccounts.GetAll(mapper.Map<Expression<Func<UserAccountInfoEntity, object>>[]>(includeProperties)).ProjectTo<UserAccountInfo>(mapper.ConfigurationProvider);
        }

        public IQueryable<UserAccountInfo> GetAllUserAccounts(Func<UserAccountInfo, bool> predicate, params Expression<Func<UserAccountInfo, object>>[] includeProperties)
        {
            return UoW.UserAccounts.GetAll(mapper.Map<Func<UserAccountInfoEntity, bool>>(predicate), mapper.Map<Expression<Func<UserAccountInfoEntity, object>>[]>(includeProperties)).ProjectTo<UserAccountInfo>(mapper.ConfigurationProvider);
        }
    }
}
