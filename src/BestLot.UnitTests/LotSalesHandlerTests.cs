using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BestLot.BusinessLogicLayer.Interfaces;
using BestLot.BusinessLogicLayer.Models;
using BestLot.DataAccessLayer.UnitOfWork;
using NUnit.Framework;

namespace BestLot.UnitTests
{
    public class LotSalesHandlerTests
    {
        private ILotOperationsHandler lotOperationsHandler;
        private IUserAccountOperationsHandler userAccountOperationsHandler;
        private IUnitOfWork unitOfWork;
        private ILotSalesHandler lotSalesHandler;

        [SetUp]
        public void SetUp()
        {
            unitOfWork = UnitTestDependencyResolver.ResolveUnitOfWork();
            lotOperationsHandler = UnitTestDependencyResolver.ResolveLotOperationsHandler(unitOfWork);
            lotSalesHandler = UnitTestDependencyResolver.ResolveLotSalesHandler(unitOfWork, 1500, 500, "", "");
            userAccountOperationsHandler = UnitTestDependencyResolver.ResolveUserAccountOperationsHandler(unitOfWork);
            userAccountOperationsHandler.AddUserAccount(new UserAccountInfo { Name = "DefaultUser", Email = "veklich99@mail.ru" });
            userAccountOperationsHandler.AddUserAccount(new UserAccountInfo { Name = "DefaultUser2", Email = "veklich99@gmail.com" });
        }

        [TearDown]
        public void TearDown()
        {
            lotSalesHandler.StopSalesHandler();
            unitOfWork.RecreateDB();
            unitOfWork.RecreateArchive();
            lotOperationsHandler.Dispose();
            userAccountOperationsHandler.Dispose();
        }

        [Test]
        public void RefreshLots_Timer1700Millisec_AddsAllLotsToDictionary()
        {
            var lot = new Lot { SellDate = DateTime.Now.AddSeconds(2), StartDate = DateTime.Now, SellerUserId = "veklich99@mail.ru", BuyerUserId = "veklich99@gmail.com" };
            lotOperationsHandler.AddLot(lot, "", "");

            lotSalesHandler.RunSalesHandler();
            Assert.That(() => lotSalesHandler.LotId_SellDatePairs.Count, Is.EqualTo(1).After(1700));
        }

        [Test]
        public void Timers_DBContains2Lots_TimersWorkRight()
        {
            var lot = new Lot { SellDate = DateTime.Now.AddSeconds(2), StartDate = DateTime.Now, SellerUserId = "veklich99@mail.ru", BuyerUserId = "veklich99@gmail.com" };
            lotOperationsHandler.AddLot(lot, "", "");
            var lot2 = new Lot { SellDate = DateTime.Now.AddSeconds(10), StartDate = DateTime.Now, SellerUserId = "veklich99@mail.ru", BuyerUserId = "veklich99@gmail.com" };
            lotOperationsHandler.AddLot(lot2, "", "");

            lotSalesHandler.RunSalesHandler();

            Assert.That(() => lotSalesHandler.LotId_SellDatePairs.Count, Is.EqualTo(2).After(1800));
            Assert.AreEqual(0, unitOfWork.LotArchive.GetAll().Count());
            Assert.AreEqual(2, unitOfWork.Lots.GetAll().Count());
            Assert.That(() => lotSalesHandler.LotId_SellDatePairs.Count, Is.EqualTo(1).After(500));
            Assert.AreEqual(1, unitOfWork.LotArchive.GetAll().Count());
            Assert.AreEqual(1, unitOfWork.Lots.GetAll().Count());
            Assert.That(() => lotSalesHandler.LotId_SellDatePairs.Count, Is.EqualTo(0).After(8000));
            Assert.AreEqual(2, unitOfWork.LotArchive.GetAll().Count());
            Assert.AreEqual(0, unitOfWork.Lots.GetAll().Count());
        }
    }
}
