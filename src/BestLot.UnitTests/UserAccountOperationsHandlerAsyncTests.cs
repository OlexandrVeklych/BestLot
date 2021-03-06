﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BestLot.BusinessLogicLayer.Exceptions;
using BestLot.BusinessLogicLayer.Interfaces;
using BestLot.BusinessLogicLayer.Models;
using BestLot.DataAccessLayer.UnitOfWork;
using NUnit.Framework;

namespace BestLot.UnitTests
{
    public class UserAccountOperationsHandlerAsyncTests
    {
        private IUserAccountOperationsHandler userAccountOperationsHandler;
        private ILotCommentOperationsHandler lotCommentOperationsHandler;
        private ILotOperationsHandler lotOperationsHandler;
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
            lotOperationsHandler.Dispose();
            userAccountOperationsHandler.Dispose();
            lotCommentOperationsHandler.Dispose();
        }

        [Test]
        public async Task AddUserAccountAsync_ValidInput_AddsUserToDB()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru", TelephoneNumber = "+380678522221" };

            await userAccountOperationsHandler.AddUserAccountAsync(user);

            var resultUser = await userAccountOperationsHandler.GetUserAccountAsync("veklich99@mail.ru");
            Assert.AreEqual(1, (await userAccountOperationsHandler.GetAllUserAccountsAsync()).Count());
            Assert.AreEqual("User1", resultUser.Name);
        }

        [Test]
        public void AddUserAccountAsync_InvalidEmailFormat_ThrowsWrongModelException()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "Kek" };

            Assert.ThrowsAsync<WrongModelException>(() => userAccountOperationsHandler.AddUserAccountAsync(user));
        }

        [Test]
        public void AddUserAccountAsync_InvalidTelephoneNumberFormat_ThrowsWrongModelException()
        {
            var user = new UserAccountInfo { Name = "User1", TelephoneNumber = "+388", Email = "veklich99@mail.ru" };

            Assert.ThrowsAsync<WrongModelException>(() => userAccountOperationsHandler.AddUserAccountAsync(user));
        }

        [Test]
        public async Task AddUserAccountAsync_RepeatedEmail_ThrowsWrongModelException()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru" };
            await userAccountOperationsHandler.AddUserAccountAsync(user);
            var user2 = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru" };

            Assert.ThrowsAsync<WrongModelException>(() => userAccountOperationsHandler.AddUserAccountAsync(user2));
        }

        [Test]
        public async Task AddUserAccountAsync_RepeatedTelephoneNumber_ThrowsWrongModelException()
        {
            var user = new UserAccountInfo { Name = "User1", TelephoneNumber = "+380678522221", Email = "veklich99@mail.ru" };
            await userAccountOperationsHandler.AddUserAccountAsync(user);
            var user2 = new UserAccountInfo { Name = "User1", TelephoneNumber = "+380678522221", Email = "veklich99@mail.ru" };

            Assert.ThrowsAsync<WrongModelException>(() => userAccountOperationsHandler.AddUserAccountAsync(user2));
        }

        [Test]
        public async Task DeleteUserAccountAsync_ValidId_DeletesWithCommentsAndLots()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru" };
            var comment1 = new LotComment { Message = "Message1", UserId = "veklich99@mail.ru", LotId = 1 };
            var comment2 = new LotComment { Message = "Message2", UserId = "veklich99@mail.ru", LotId = 1 };
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", StartDate = DateTime.Now, SellDate = DateTime.Now.AddDays(1), };
            await userAccountOperationsHandler.AddUserAccountAsync(user);
            await lotOperationsHandler.AddLotAsync(lot, "", "");
            await lotCommentOperationsHandler.AddCommentAsync(comment1);
            await lotCommentOperationsHandler.AddCommentAsync(comment2);

            await userAccountOperationsHandler.DeleteUserAccountAsync("veklich99@mail.ru", "", "");

            Assert.AreEqual(0, (await userAccountOperationsHandler.GetAllUserAccountsAsync()).Count());
            //Lot was deleted together with user, so lot with Id = 1 doesn`t exist
            Assert.ThrowsAsync<WrongIdException>(() => lotOperationsHandler.GetLotAsync(1));
        }

        [Test]
        public async Task DeleteUserAccountAsync_InvalidId_ThrowsWrongIdException()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru" };
            await userAccountOperationsHandler.AddUserAccountAsync(user);

            Assert.ThrowsAsync<WrongIdException>(() => userAccountOperationsHandler.DeleteUserAccountAsync("veklich99@gmail.com", "", ""));
        }

        [Test]
        public async Task ChangeUserAccountAsync_ChangedNameAndComments_NameChangedCommentsNotChanged()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru" };
            await userAccountOperationsHandler.AddUserAccountAsync(user);

            var changedUser = await userAccountOperationsHandler.GetUserAccountAsync("veklich99@mail.ru");
            changedUser.LotComments = new List<LotComment> { new LotComment { Message = "Message1", LotId = 1, UserId = "veklich99@mail.ru" } };
            changedUser.Name = "User2";
            await userAccountOperationsHandler.ChangeUserAccountAsync("veklich99@mail.ru", changedUser);

            var resultUser = await userAccountOperationsHandler.GetUserAccountAsync("veklich99@mail.ru");
            resultUser.LotComments = (await lotCommentOperationsHandler.GetUserCommentsAsync("veklich99@mail.ru")).ToList();

            Assert.AreEqual("User2", resultUser.Name);
            Assert.AreEqual(0, resultUser.LotComments.Count());
        }

        [Test]
        public async Task ChangeUserAccountAsync_InvalidUser_ThrowsExceptions()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru" };
            await userAccountOperationsHandler.AddUserAccountAsync(user);

            var changedUser = await userAccountOperationsHandler.GetUserAccountAsync("veklich99@mail.ru");
            changedUser.Email = "changedEmail";

            Assert.ThrowsAsync<WrongIdException>(() => userAccountOperationsHandler.ChangeUserAccountAsync("veklich99@gmail.com", changedUser));//Invalid email
            Assert.ThrowsAsync<WrongModelException>(() => userAccountOperationsHandler.ChangeUserAccountAsync("veklich99@mail.ru", changedUser));//Changed email
        }

        [Test]
        public async Task GetAllLotsAsync_DBContains1Elem_CountReturns1Elem()
        {
            var user = new UserAccountInfo { Name = "User1", Email = "veklich99@mail.ru" };
            await userAccountOperationsHandler.AddUserAccountAsync(user);

            var resultUsers = await userAccountOperationsHandler.GetAllUserAccountsAsync();

            Assert.AreEqual(1, resultUsers.Count());
        }
    }
}