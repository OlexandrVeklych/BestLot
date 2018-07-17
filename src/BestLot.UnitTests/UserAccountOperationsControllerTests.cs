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
    }
}
