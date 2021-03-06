﻿using System;
using System.Collections.Generic;
using System.Linq;
using BestLot.BusinessLogicLayer.Exceptions;
using BestLot.BusinessLogicLayer.Interfaces;
using BestLot.BusinessLogicLayer.Models;
using BestLot.DataAccessLayer.UnitOfWork;
using NUnit.Framework;

namespace BestLot.UnitTests
{
    [TestFixture]
    public class LotOperationsHandlerTests
    {
        private ILotOperationsHandler lotOperationsHandler;
        private IUserAccountOperationsHandler userAccountOperationsHandler;
        private ILotCommentOperationsHandler lotCommentOperationsHandler;
        private IUnitOfWork unitOfWork;

        [SetUp]
        public void SetUp()
        {
            unitOfWork = UnitTestDependencyResolver.ResolveUnitOfWork();
            lotOperationsHandler = UnitTestDependencyResolver.ResolveLotOperationsHandler(unitOfWork);
            lotCommentOperationsHandler = UnitTestDependencyResolver.ResolveLotCommentOperationsHandler(unitOfWork);
            userAccountOperationsHandler = UnitTestDependencyResolver.ResolveUserAccountOperationsHandler(unitOfWork);
            userAccountOperationsHandler.AddUserAccount(new UserAccountInfo { Name = "DefaultUser", Email = "veklich99@mail.ru" });
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
        public void AddLot_ValidInput_AddsLotToDB()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now };

            lotOperationsHandler.AddLot(lot, "", "");

            Assert.AreEqual(1, lotOperationsHandler.GetAllLots().Count());
        }

        [Test]
        public void AddLot_InvalidInput_ThrowsExceptions()
        {
            var invalidEmailLot = new Lot { SellerUserId = "veklich99@gmail.com", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now };
            var invalidDatesLot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, StartDate = DateTime.Now.AddDays(1) };

            Assert.ThrowsAsync<WrongIdException>(() => lotOperationsHandler.AddLotAsync(invalidEmailLot, "", ""));
            Assert.ThrowsAsync<WrongModelException>(() => lotOperationsHandler.AddLotAsync(invalidDatesLot, "", ""));
        }

        [Test]
        public void GetLot_GetWithId2_ReturnsCorrectElem()
        {
            var lot1 = new Lot { SellerUserId = "veklich99@mail.ru", Name = "Name1", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now };
            var lot2 = new Lot { SellerUserId = "veklich99@mail.ru", Name = "Name2", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now };
            lotOperationsHandler.AddLot(lot1, "", "");
            lotOperationsHandler.AddLot(lot2, "", "");

            var resultLot = lotOperationsHandler.GetLot(2);

            Assert.AreEqual("Name2", resultLot.Name);
        }

        [Test]
        public void GetLot_InvalidId_ThrowsWrongIdException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", Name = "Name1", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now, LotComments = new List<LotComment> { new LotComment { LotId = 1, UserId = "veklich99@mail.ru", Message = "Message1" } } };
            lotOperationsHandler.AddLot(lot, "", "");

            Assert.Throws<WrongIdException>(() => lotOperationsHandler.GetLot(2));
        }

        [Test]
        public void GetAllLots_DBContains1Elem_CountReturns1Elem()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now };
            lotOperationsHandler.AddLot(lot, "", "");

            var resultLots = lotOperationsHandler.GetAllLots();

            Assert.AreEqual(1, resultLots.Count());
        }

        [Test]
        public void ChangeLot_ChangedNameAndComments_NameChangedCommentsNotChanged()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now, Name = "Name1", LotComments = new List<LotComment> { new LotComment { Message = "Message1", LotId = 1, UserId = "veklich99@mail.ru" } } };
            lotOperationsHandler.AddLot(lot, "", "");

            var lotComment = new LotComment { Message = "Message2", LotId = 1, UserId = "veklich99@mail.ru" };
            var modifiedLot = lotOperationsHandler.GetLot(1);
            modifiedLot.LotComments = new List<LotComment> { lotComment };
            modifiedLot.Name = "Name2";
            lotOperationsHandler.ChangeLot(1, modifiedLot);

            var resultLot = lotOperationsHandler.GetLot(1);
            resultLot.LotComments = lotCommentOperationsHandler.GetLotComments(1).ToList();

            Assert.AreEqual(1, lotOperationsHandler.GetAllLots().Count());
            Assert.AreEqual(1, resultLot.LotComments.Count());
            Assert.AreEqual("Name2", resultLot.Name);
            Assert.AreEqual("Message1", resultLot.LotComments[0].Message);
        }

        [Test]
        public void ChangeLot_InvalidLot_ThrowsWrongModelException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot, "", "");

            var modifiedLot = lotOperationsHandler.GetLot(1);

            //Change id - not allowed
            modifiedLot.Id = 2;
            Assert.Throws<WrongModelException>(() => lotOperationsHandler.ChangeLot(1, modifiedLot));

            //Reset id, change seller user - not allowed
            modifiedLot.Id = 1;
            modifiedLot.SellerUserId = "veklich99@gmail.com";
            Assert.Throws<WrongModelException>(() => lotOperationsHandler.ChangeLot(1, modifiedLot));

            //Reset seller user, change buyer user - not allowed
            modifiedLot.SellerUserId = "veklich99@mail.ru";
            modifiedLot.BuyerUserId = "veklich99@mail.ru";
            Assert.Throws<WrongModelException>(() => lotOperationsHandler.ChangeLot(1, modifiedLot));
        }

        [Test]
        public void DeleteLot_ValidId_DeletesInDB()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot, "", "");

            lotOperationsHandler.DeleteLot(1, "", "");

            Assert.AreEqual(0, lotOperationsHandler.GetAllLots().Count());
        }

        [Test]
        public void DeleteLot_InvalidId_ThrowsWrongIdException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot, "", "");

            Assert.Throws<WrongIdException>(() => lotOperationsHandler.DeleteLot(2, "", ""));
        }

        [Test]
        public void PlaceBid_ValidInput_ChangesPriceAndBuyerUserId()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now, Name = "Name1" };
            var user = new UserAccountInfo { Name = "SecondUser", Email = "veklich98@mail.ru" };
            lotOperationsHandler.AddLot(lot, "", "");
            userAccountOperationsHandler.AddUserAccount(user);

            lotOperationsHandler.PlaceBid(1, "veklich98@mail.ru", 15);

            var resultLot = lotOperationsHandler.GetLot(1);
            Assert.AreEqual(1, lotOperationsHandler.GetAllLots().Count());
            Assert.AreEqual(15, resultLot.Price);
            Assert.AreEqual("veklich98@mail.ru", resultLot.BuyerUserId);
        }

        [Test]
        public void PlaceBid_InvalidInput_ThrowsExceptions()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now, Name = "Name1", Price = 10, MinStep = 10 };
            lotOperationsHandler.AddLot(lot, "", "");
            var user = new UserAccountInfo { Name = "SecondUser", Email = "veklich98@mail.ru" };
            userAccountOperationsHandler.AddUserAccount(user);

            Assert.Throws<WrongModelException>(() => lotOperationsHandler.PlaceBid(1, "veklich98@mail.ru", 15));//price < price + step
            Assert.Throws<WrongModelException>(() => lotOperationsHandler.PlaceBid(1, "veklich99@mail.ru", 25));//Bid for own lot
            Assert.Throws<WrongIdException>(() => lotOperationsHandler.PlaceBid(1, "veklich98@gmail.com", 25));//Invalid email
            Assert.Throws<WrongIdException>(() => lotOperationsHandler.PlaceBid(3, "veklich98@mail.ru", 25));//Invalid lot id
        }

        [Test]
        public void PlaceBid_RelativeBidplacer_ChangesPriceAndBuyerUserId()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now, Name = "Name1", BidPlacer = "Relative" };
            var user = new UserAccountInfo { Name = "SecondUser", Email = "veklich98@mail.ru" };
            lotOperationsHandler.AddLot(lot, "", "");
            userAccountOperationsHandler.AddUserAccount(user);

            lotOperationsHandler.PlaceBid(1, "veklich98@mail.ru", 15);

            var resultLot = lotOperationsHandler.GetLot(1);

            Assert.AreEqual(1, lotOperationsHandler.GetAllLots().Count());
            Assert.AreEqual(15, resultLot.Price);
            Assert.AreEqual("veklich98@mail.ru", resultLot.BuyerUserId);
            Assert.AreEqual(-1, lot.StartDate.CompareTo(resultLot.StartDate));//Start date moved forward
            Assert.AreEqual(-1, lot.SellDate.CompareTo(resultLot.SellDate));//Sell date moved forward
        }

        [Test]
        public void PlaceBid_RelativeBidplacerInvalidInput_ThrowsExceptions()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now, Name = "Name1", BidPlacer = "Relative", Price = 10, MinStep = 10 };
            var user = new UserAccountInfo { Name = "SecondUser", Email = "veklich98@mail.ru" };
            lotOperationsHandler.AddLot(lot, "", "");
            userAccountOperationsHandler.AddUserAccount(user);

            Assert.Throws<WrongModelException>(() => lotOperationsHandler.PlaceBid(1, "veklich98@mail.ru", 15));//price < price + step
            Assert.Throws<WrongModelException>(() => lotOperationsHandler.PlaceBid(1, "veklich99@mail.ru", 25));//Bid for own lot
            Assert.Throws<WrongIdException>(() => lotOperationsHandler.PlaceBid(1, "veklich98@gmail.com", 25));//Invalid email
            Assert.Throws<WrongIdException>(() => lotOperationsHandler.PlaceBid(3, "veklich99@mail.ru", 25));//Invalid lot id
        }
    }
}
