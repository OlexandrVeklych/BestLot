using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.UnitOfWork;
using DataAccessLayer.Entities;
using BusinessLogicLayer.Models;
using AutoMapper;

namespace BusinessLogicLayer
{
    public class UserAccountOperationsController
    {
        public UserAccountOperationsController(IUnitOfWork unitOfWork)
        {
            UoW = unitOfWork;
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserAccountInfo, UserAccountInfoModel>();
                cfg.CreateMap<UserAccountInfoModel, UserAccountInfo>();
            }).CreateMapper();
        }

        private IUnitOfWork UoW;
        private IMapper mapper;

        public void AddUserAccount(UserAccountInfoModel userAccount)
        {
            UoW.UserAccounts.Add(mapper.Map<UserAccountInfo>(userAccount));
            UoW.SaveChanges();
        }

        public void DeleteUserAccount(int userAccountId)
        {
            UoW.UserAccounts.Delete(userAccountId);
            UoW.SaveChanges();
        }
    }
}
