using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BestLot.BusinessLogicLayer.LogicHandlers;
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
            lotOperationsHandler = UnitTestDependencyResolver.ResloveLotOperationsHandler(unitOfWork);
            lotSalesHandler = UnitTestDependencyResolver.ResolveLotSalesHandler(unitOfWork, 1500, 500);
            userAccountOperationsHandler = UnitTestDependencyResolver.ResloveUserAccountOperationsHandler(unitOfWork);
            userAccountOperationsHandler.AddUserAccount(new UserAccountInfo { Name = "DefaultUser" });
            userAccountOperationsHandler.AddUserAccount(new UserAccountInfo { Name = "DefaultUser2" });
        }

        [TearDown]
        public void TearDown()
        {
            lotSalesHandler.StopSalesHandler();
            unitOfWork.RecreateDB();
            unitOfWork.RecreateArchive();
        }

        [Test]
        public void RefreshLots_Timer1700Millisec_AddsAllLotsToDictionary()
        {
            var lot = new Lot { SellDate = DateTime.Now.AddSeconds(2), SellerUserId = 1, BuyerUserId = 2 };
            lotOperationsHandler.AddLot(lot);

            lotSalesHandler.RunSalesHandler();
            Assert.That(() => lotSalesHandler.lotsSellDate.Count, Is.EqualTo(1).After(1700));
        }

        [Test]
        public void Timers_DBContains2Lots_TimersWorkRight()
        {
            var lot = new Lot { SellDate = DateTime.Now.AddSeconds(2), SellerUserId = 1, BuyerUserId = 2 };
            lotOperationsHandler.AddLot(lot);
            var lot2 = new Lot { SellDate = DateTime.Now.AddSeconds(10), SellerUserId = 1, BuyerUserId = 2 };
            lotOperationsHandler.AddLot(lot2);

            lotSalesHandler.RunSalesHandler();

            Assert.That(() => lotSalesHandler.lotsSellDate.Count, Is.EqualTo(2).After(1700));
            Assert.AreEqual(0, unitOfWork.LotArchive.GetAll().Count());
            Assert.AreEqual(2, unitOfWork.Lots.GetAll().Count());
            Assert.That(() => lotSalesHandler.lotsSellDate.Count, Is.EqualTo(1).After(500));
            Assert.AreEqual(1, unitOfWork.LotArchive.GetAll().Count());
            Assert.AreEqual(1, unitOfWork.Lots.GetAll().Count());
            Assert.That(() => lotSalesHandler.lotsSellDate.Count, Is.EqualTo(0).After(8000));
            Assert.AreEqual(2, unitOfWork.LotArchive.GetAll().Count());
            Assert.AreEqual(0, unitOfWork.Lots.GetAll().Count());
        }
    }
}
