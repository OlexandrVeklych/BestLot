using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogicLayer.LogicHandlers;
using BusinessLogicLayer.Models;
using DataAccessLayer.Entities;
using DataAccessLayer.UnitOfWork;
using NUnit.Framework;

namespace UnitTests
{
    public class UserAccountOperationsControllerTests
    {
        private UserAccountOperationsHandler userAccountOperationsHandler;
        private IUnitOfWork unitOfWork;

        [SetUp]
        public void SetUp()
        {
            unitOfWork = UnitTestDependencyResolver.Resolve();
            userAccountOperationsHandler = new UserAccountOperationsHandler(unitOfWork);
        }

        [TearDown]
        public void TearDown()
        {
            unitOfWork.RecreateDB();
        }

        [Test]
        public void AddUserAccount_ValidInput_AddsUserToDB()
        {
            var user = new UserAccountInfo { Name = "User1" };

            userAccountOperationsHandler.AddUserAccount(user);

            var resultUser = userAccountOperationsHandler.GetUserAccount(1);
            Assert.AreEqual(1, userAccountOperationsHandler.GetAllUserAccounts().Count());
            Assert.AreEqual("User1", resultUser.Name);
        }

        [Test]
        public void DeleteUserAccount_ValidId_DeletesWithCommentsAndLots()
        {
            var lotOperationsHandler = new LotOperationsHandler(unitOfWork);
            var user = new UserAccountInfo { Name = "User1" };
            var comment1 = new LotComment { Message = "Message1", UserId = 1, LotId = 1 };
            var comment2 = new LotComment { Message = "Message2", UserId = 1, LotId = 1 };
            var lot = new Lot { SellerUserId = 1, StartDate = DateTime.Now, SellDate = DateTime.Now, };
            userAccountOperationsHandler.AddUserAccount(user);
            lotOperationsHandler.AddLot(lot);
            lotOperationsHandler.AddComment(comment1);
            lotOperationsHandler.AddComment(comment2);

            userAccountOperationsHandler.DeleteUserAccount(1);

            Assert.AreEqual(0, userAccountOperationsHandler.GetAllUserAccounts().Count());
            Assert.Throws<ArgumentException>(() => lotOperationsHandler.GetLot(1, l=> lot.Comments));
        }
    }
}
