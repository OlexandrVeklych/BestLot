using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogicLayer.LogicHandlers;
using BusinessLogicLayer.Models;
using DataAccessLayer.UnitOfWork;
using NUnit.Framework;

namespace UnitTests
{
    public class UserAccountOperationsHandlerTests
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
            var user = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru", TelephoneNumber = "+380678522221" };

            userAccountOperationsHandler.AddUserAccount(user);

            var resultUser = userAccountOperationsHandler.GetUserAccount(1);
            Assert.AreEqual(1, userAccountOperationsHandler.GetAllUserAccounts().Count());
            Assert.AreEqual("User1", resultUser.Name);
        }

        [Test]
        public void AddUserAccount_InvalidEmailFormat_ThrowsArgumentException()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "Kek" };

            Assert.Throws<ArgumentException>(()=> userAccountOperationsHandler.AddUserAccount(user));
        }

        [Test]
        public void AddUserAccount_InvalidTelephoneNumberFormat_ThrowsArgumentException()
        {
            var user = new UserAccountInfo { Name = "User1", TelephoneNumber = "+388" };

            Assert.Throws<ArgumentException>(() => userAccountOperationsHandler.AddUserAccount(user));
        }

        [Test]
        public void AddUserAccount_RepeatedEmail_ThrowsArgumentException()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru" };
            userAccountOperationsHandler.AddUserAccount(user);
            var user2 = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru" };

            Assert.Throws<ArgumentException>(() => userAccountOperationsHandler.AddUserAccount(user2));
        }

        [Test]
        public void AddUserAccount_RepeatedTelephoneNumber_ThrowsArgumentException()
        {
            var user = new UserAccountInfo { Name = "User1", TelephoneNumber = "+380678522221" };
            userAccountOperationsHandler.AddUserAccount(user);
            var user2 = new UserAccountInfo { Name = "User1", TelephoneNumber = "+380678522221" };

            Assert.Throws<ArgumentException>(() => userAccountOperationsHandler.AddUserAccount(user2));
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
            //Lot was deleted together with user, so lot with Id = 1 doesn`t exist
            Assert.Throws<ArgumentException>(() => lotOperationsHandler.GetLot(1, l=> lot.Comments));
        }

        [Test]
        public void DeleteUserAccount_InvalidId_ThrowsArgumentException()
        {
            var user = new UserAccountInfo { Name = "User1" };
            userAccountOperationsHandler.AddUserAccount(user);

            Assert.Throws<ArgumentException>(() => userAccountOperationsHandler.DeleteUserAccount(2));
        }

        [Test]
        public void ChangeUserAccount_ChangedNameAndComments_NameChangedCommentsNotChanged()
        {
            var user = new UserAccountInfo { Name = "User1" };
            userAccountOperationsHandler.AddUserAccount(user);

            var changedUser = userAccountOperationsHandler.GetUserAccount(1);
            changedUser.LotComments = new List<LotComment> { new LotComment { Message = "Message1", LotId = 1, UserId = 1 } };
            changedUser.Name = "User2";
            userAccountOperationsHandler.ChangeUserAccount(1, changedUser);

            var resultUser = userAccountOperationsHandler.GetUserAccount(1, u => u.LotComments, u => u.Lots);
            Assert.AreEqual("User2", resultUser.Name);
            Assert.AreEqual(0, resultUser.LotComments.Count());
        }

        [Test]
        public void ChangeUserAccount_InvalidUser_ThrowsArgumentException()
        {
            var user = new UserAccountInfo { Name = "User1" };
            userAccountOperationsHandler.AddUserAccount(user);

            var changedUser = userAccountOperationsHandler.GetUserAccount(1);
            changedUser.Name = "User2";

            Assert.Throws<ArgumentException>(() => userAccountOperationsHandler.ChangeUserAccount(2, changedUser));
        }

        [Test]
        public void GetAllLots_DBContains1Elem_CountReturns1Elem()
        {
            var user = new UserAccountInfo { Name = "User1" };
            userAccountOperationsHandler.AddUserAccount(user);

            var resultUsers = userAccountOperationsHandler.GetAllUserAccounts();

            Assert.AreEqual(1, resultUsers.Count());
        }

        [Test]
        public void GetAllUserAccounts_WithInclude_ReturnsObjectWithInnerProperties()
        {
            var lotOperationsHandler = new LotOperationsHandler(unitOfWork);
            var user = new UserAccountInfo { Name = "User1" };
            userAccountOperationsHandler.AddUserAccount(user);
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { Message = "Message1", LotId = 1, UserId = 1 } } };
            lotOperationsHandler.AddLot(lot);

            var resultUsers = userAccountOperationsHandler.GetAllUserAccounts(u => u.LotComments);

            Assert.AreEqual("Message1", resultUsers.ToList()[0].LotComments[0].Message);
        }

        [Test]
        public void GetAllUserAccounts_WithIncludeAndFilter_ReturnsFilteredObjectWithInnerProperties()
        {
            var lotOperationsHandler = new LotOperationsHandler(unitOfWork);
            var user = new UserAccountInfo { Name = "User1" };
            userAccountOperationsHandler.AddUserAccount(user);
            var user2 = new UserAccountInfo { Name = "User2" };
            userAccountOperationsHandler.AddUserAccount(user2);
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { Message = "Message1", LotId = 1, UserId = 1 } } };
            lotOperationsHandler.AddLot(lot);
            lotOperationsHandler.AddComment(new LotComment { Message = "Message2", LotId = 1, UserId = 2 });

            var resultUsers = userAccountOperationsHandler.GetAllUserAccounts(u => u.LotComments).Where(u => u.LotComments.Where(c => c.Message == "Message2").Count() > 0);

            Assert.AreEqual(1, resultUsers.Count());
            Assert.AreEqual("Message2", resultUsers.ToList()[0].LotComments[0].Message);
        }
    }
}