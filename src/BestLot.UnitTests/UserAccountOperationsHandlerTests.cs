﻿using System;
using System.Collections.Generic;
using System.Linq;
using BestLot.BusinessLogicLayer.Interfaces;
using BestLot.BusinessLogicLayer.Models;
using BestLot.DataAccessLayer.UnitOfWork;
using NUnit.Framework;

namespace BestLot.UnitTests
{
    public class UserAccountOperationsHandlerTests
    {
        private IUserAccountOperationsHandler userAccountOperationsHandler;
        private ILotOperationsHandler lotOperationsHandler;
        private ILotCommentOperationsHandler lotCommentOperationsHandler;
        private IUnitOfWork unitOfWork;

        [SetUp]
        public void SetUp()
        {
            unitOfWork = UnitTestDependencyResolver.ResolveUnitOfWork();
            lotOperationsHandler = UnitTestDependencyResolver.ResolveLotOperationsHandler(unitOfWork);
            userAccountOperationsHandler = UnitTestDependencyResolver.ResolveUserAccountOperationsHandler(unitOfWork);
            lotCommentOperationsHandler = UnitTestDependencyResolver.ResolveLotCommentOperationsHandler(unitOfWork);
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

            var resultUser = userAccountOperationsHandler.GetUserAccount("veklich99@mail.ru");
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
            var user = new UserAccountInfo { Name = "User1", TelephoneNumber = "+388", Email = "veklich99@mail.ru" };

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
            var user = new UserAccountInfo { Name = "User1", TelephoneNumber = "+380678522221", Email = "veklich99@mail.ru" };
            userAccountOperationsHandler.AddUserAccount(user);
            var user2 = new UserAccountInfo { Name = "User1", TelephoneNumber = "+380678522221", Email = "veklich99@mail.ru" };

            Assert.Throws<ArgumentException>(() => userAccountOperationsHandler.AddUserAccount(user2));
        }

        [Test]
        public void DeleteUserAccount_ValidId_DeletesWithCommentsAndLots()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru" };
            var comment1 = new LotComment { Message = "Message1", UserId = "veklich99@mail.ru", LotId = 1 };
            var comment2 = new LotComment { Message = "Message2", UserId = "veklich99@mail.ru", LotId = 1 };
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", StartDate = DateTime.Now, SellDate = DateTime.Now, };
            userAccountOperationsHandler.AddUserAccount(user);
            lotOperationsHandler.AddLot(lot, "", "");
            lotCommentOperationsHandler.AddComment(comment1);
            lotCommentOperationsHandler.AddComment(comment2);

            userAccountOperationsHandler.DeleteUserAccount("veklich99@mail.ru", "", "");

            Assert.AreEqual(0, userAccountOperationsHandler.GetAllUserAccounts().Count());
            //Lot was deleted together with user, so lot with Id = 1 doesn`t exist
            Assert.Throws<ArgumentException>(() => lotOperationsHandler.GetLot(1));
        }

        [Test]
        public void DeleteUserAccount_InvalidId_ThrowsArgumentException()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru" };
            userAccountOperationsHandler.AddUserAccount(user);

            Assert.Throws<ArgumentException>(() => userAccountOperationsHandler.DeleteUserAccount("veklich99@gmail.com", "", ""));
        }

        [Test]
        public void ChangeUserAccount_ChangedNameAndComments_NameChangedCommentsNotChanged()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru" };
            userAccountOperationsHandler.AddUserAccount(user);

            var changedUser = userAccountOperationsHandler.GetUserAccount("veklich99@mail.ru");
            changedUser.LotComments = new List<LotComment> { new LotComment { Message = "Message1", LotId = 1, UserId = "veklich99@mail.ru" } };
            changedUser.Name = "User2";
            userAccountOperationsHandler.ChangeUserAccount("veklich99@mail.ru", changedUser);

            var resultUser = userAccountOperationsHandler.GetUserAccount("veklich99@mail.ru", u => u.LotComments, u => u.Lots);
            Assert.AreEqual("User2", resultUser.Name);
            Assert.AreEqual(0, resultUser.LotComments.Count());
        }

        [Test]
        public void ChangeUserAccount_InvalidUser_ThrowsArgumentException()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru" };
            userAccountOperationsHandler.AddUserAccount(user);

            var changedUser = userAccountOperationsHandler.GetUserAccount("veklich99@mail.ru");
            changedUser.Name = "User2";

            Assert.Throws<ArgumentException>(() => userAccountOperationsHandler.ChangeUserAccount("veklich99@gmail.com", changedUser));
        }

        [Test]
        public void GetAllLots_DBContains1Elem_CountReturns1Elem()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru" };
            userAccountOperationsHandler.AddUserAccount(user);

            var resultUsers = userAccountOperationsHandler.GetAllUserAccounts();

            Assert.AreEqual(1, resultUsers.Count());
        }

        [Test]
        public void GetAllUserAccounts_WithInclude_ReturnsObjectWithInnerProperties()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru" };
            userAccountOperationsHandler.AddUserAccount(user);
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Lot1", LotComments = new List<LotComment> { new LotComment { Message = "Message1", LotId = 1, UserId = "veklich99@mail.ru" } } };
            lotOperationsHandler.AddLot(lot, "", "");

            var resultUsers = userAccountOperationsHandler.GetAllUserAccounts().ToList();
            var resultUser = userAccountOperationsHandler.GetUserAccount("veklich99@mail.ru");
            resultUser.Lots = lotOperationsHandler.GetUserLots(resultUser.Email).ToList();
            resultUser.LotComments = lotCommentOperationsHandler.GetUserComments(resultUser.Email).ToList();

            Assert.AreEqual("Lot1", resultUser.Lots[0].Name);
            Assert.AreEqual("Message1", resultUser.LotComments[0].Message);
            Assert.AreEqual("Lot1", resultUsers[0].Lots[0].Name);
            Assert.AreEqual("Message1", resultUsers[0].LotComments[0].Message);
        }

        [Test]
        public void GetAllUserAccounts_WithIncludeAndFilter_ReturnsFilteredObjectWithInnerProperties()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru" };
            userAccountOperationsHandler.AddUserAccount(user);
            var user2 = new UserAccountInfo { Name = "User2", Email = "veklich99@gmail.com" };
            userAccountOperationsHandler.AddUserAccount(user2);
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, LotComments = new List<LotComment> { new LotComment { Message = "Message1", LotId = 1, UserId = "veklich99@mail.ru" } } };
            lotOperationsHandler.AddLot(lot, "", "");
            lotCommentOperationsHandler.AddComment(new LotComment { Message = "Message2", LotId = 1, UserId = "veklich99@gmail.com" });
        
            var resultUsers = userAccountOperationsHandler.GetAllUserAccounts(u => u.LotComments).Where(u => u.LotComments.Where(c => c.Message == "Message2").Count() > 0);
        
            Assert.AreEqual(1, resultUsers.Count());
            Assert.AreEqual("Message2", resultUsers.ToList()[0].LotComments[0].Message);
        }
    }
}