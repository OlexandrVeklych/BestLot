using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BusinessLogicLayer.LogicHandlers;
using BusinessLogicLayer.Models;
using DataAccessLayer.UnitOfWork;
using NUnit.Framework;

namespace UnitTests
{
    public class LotSalesHandlerTests
    {
        private LotOperationsHandler lotOperationsHandler;
        private UserAccountOperationsHandler userAccountOperationsHandler;
        private IUnitOfWork unitOfWork;
        private LotSalesHandler lotSalesHandler;

        [SetUp]
        public void SetUp()
        {
            unitOfWork = UnitTestDependencyResolver.Resolve();
            lotOperationsHandler = new LotOperationsHandler(unitOfWork);
            lotSalesHandler = new LotSalesHandler(unitOfWork, 1500);
            userAccountOperationsHandler = new UserAccountOperationsHandler(unitOfWork);
            userAccountOperationsHandler.AddUserAccount(new UserAccountInfo { Name = "DefaultUser" });
            userAccountOperationsHandler.AddUserAccount(new UserAccountInfo { Name = "DefaultUser2" });
        }

        [TearDown]
        public void TearDown()
        {
            lotSalesHandler.Stop();
            unitOfWork.RecreateDB();
            unitOfWork.RecreateArchive();
        }

        [Test]
        public void RefreshLots_Timer1Sec_AddsAllLotsToDictionary()
        {
            var lot = new Lot { SellDate = DateTime.Now.AddSeconds(2), SellerUserId = 1, BuyerUserId = 2 };
            lotOperationsHandler.AddLot(lot);

            lotSalesHandler.Run();
            Assert.That(() => lotSalesHandler.lotsSellDate.Count, Is.EqualTo(1).After(500));
        }

        [Test]
        public void SellLot_Timer1Sec_DeletesLotFromDBAddsToArchive()
        {
            var lot = new Lot { SellDate = DateTime.Now.AddSeconds(2), SellerUserId = 1, BuyerUserId = 2 };
            lotOperationsHandler.AddLot(lot);
            var lot2 = new Lot { SellDate = DateTime.Now.AddSeconds(10), SellerUserId = 1, BuyerUserId = 2 };
            lotOperationsHandler.AddLot(lot2);

            lotSalesHandler.Run();
            Assert.That(() => lotSalesHandler.lotsSellDate.Count, Is.EqualTo(2).After(500));
            Assert.AreEqual(0, unitOfWork.LotArchive.GetAll().Count());
            Assert.AreEqual(2, unitOfWork.Lots.GetAll().Count());
            Assert.That(() => lotSalesHandler.lotsSellDate.Count, Is.EqualTo(1).After(3000));
            Assert.AreEqual(1, unitOfWork.LotArchive.GetAll().Count());
            Assert.AreEqual(1, unitOfWork.Lots.GetAll().Count());
            Assert.That(() => lotSalesHandler.lotsSellDate.Count, Is.EqualTo(0).After(8000));
            Assert.AreEqual(2, unitOfWork.LotArchive.GetAll().Count());
            Assert.AreEqual(0, unitOfWork.Lots.GetAll().Count());
        }
    }
}
