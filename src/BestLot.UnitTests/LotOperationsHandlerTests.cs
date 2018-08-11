using System;
using System.Collections.Generic;
using System.Linq;
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
        private IUnitOfWork unitOfWork;

        [SetUp]
        public void SetUp()
        {
            unitOfWork = UnitTestDependencyResolver.ResolveUnitOfWork();
            lotOperationsHandler = UnitTestDependencyResolver.ResolveLotOperationsHandler(unitOfWork);
            userAccountOperationsHandler = UnitTestDependencyResolver.ResolveUserAccountOperationsHandler(unitOfWork);
            userAccountOperationsHandler.AddUserAccount(new UserAccountInfo { Name = "DefaultUser", Email = "veklich99@mail.ru" });
        }

        [TearDown]
        public void TearDown()
        {
            unitOfWork.RecreateDB();
        }

        [Test]
        public void AddLot_ValidInput_AddsLotToDB()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, StartDate = DateTime.Now };

            lotOperationsHandler.AddLot(lot, "", "");

            Assert.AreEqual(1, lotOperationsHandler.GetAllLots().Count());
        }

        [Test]
        public void AddLot_InvalidInput_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = "veklich99@gmail.com", SellDate = DateTime.Now, StartDate = DateTime.Now };

            Assert.Throws<ArgumentException>(() => lotOperationsHandler.AddLot(lot, "", ""));
        }

        [Test]
        public void GetLotWithoutInclude_GetWithId2_ReturnsCorrectElem()
        {
            var lot1 = new Lot { SellerUserId = "veklich99@mail.ru", Name = "Name1", SellDate = DateTime.Now, StartDate = DateTime.Now };
            var lot2 = new Lot { SellerUserId = "veklich99@mail.ru", Name = "Name2", SellDate = DateTime.Now, StartDate = DateTime.Now };
            lotOperationsHandler.AddLot(lot1, "", "");
            lotOperationsHandler.AddLot(lot2, "", "");

            var resultLot = lotOperationsHandler.GetLot(2);

            Assert.AreEqual("Name2", resultLot.Name);
        }

        [Test]
        public void GetLotWithInclude_GetWithId2_ReturnsCorrectElem()
        {
            var lot1 = new Lot { SellerUserId = "veklich99@mail.ru", Name = "Name1", SellDate = DateTime.Now, LotComments = new List<LotComment> { new LotComment { LotId = 1, UserId = "veklich99@mail.ru", Message = "Message1" } } };
            var lot2 = new Lot { SellerUserId = "veklich99@mail.ru", Name = "Name2", SellDate = DateTime.Now, LotComments = new List<LotComment> { new LotComment { LotId = 1, UserId = "veklich99@mail.ru", Message = "Message2" } } };
            lotOperationsHandler.AddLot(lot1, "", "");
            lotOperationsHandler.AddLot(lot2, "", "");

            var resultLot = lotOperationsHandler.GetLot(2, l => l.LotComments, l => l.SellerUser);

            Assert.AreEqual("DefaultUser", resultLot.SellerUser.Name);
            Assert.AreEqual("Name2", resultLot.Name);
            Assert.AreEqual("Message2", resultLot.LotComments[0].Message);
        }

        [Test]
        public void GetLot_InvalidId_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", Name = "Name1", SellDate = DateTime.Now, LotComments = new List<LotComment> { new LotComment { LotId = 1, UserId = "veklich99@mail.ru", Message = "Message1" } } };
            lotOperationsHandler.AddLot(lot, "", "");

            Assert.Throws<ArgumentException>(() => lotOperationsHandler.GetLot(2));
        }

        [Test]
        public void GetAllLots_DBContains1Elem_CountReturns1Elem()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, StartDate = DateTime.Now };
            lotOperationsHandler.AddLot(lot, "", "");

            var resultLots = lotOperationsHandler.GetAllLots();

            Assert.AreEqual(1, resultLots.Count());
        }

        [Test]
        public void GetAllLots_WithInclude_ReturnsObjectWithInnerProperties()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, LotComments = new List<LotComment> { new LotComment { LotId = 1, UserId = "veklich99@mail.ru", Message = "Message1" } } };
            lotOperationsHandler.AddLot(lot, "", "");

            var resultLots = lotOperationsHandler.GetAllLots(l => l.LotComments);

            Assert.AreEqual("Message1", resultLots.ToList()[0].LotComments[0].Message);
        }

        [Test]
        public void GetAllLots_WithIncludeAndFilter_ReturnsFilteredObjectWithInnerProperties()
        {
            var lot1 = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, LotComments = new List<LotComment> { new LotComment { LotId = 1, UserId = "veklich99@mail.ru", Message = "Message1" } } };
            var lot2 = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, LotComments = new List<LotComment> { new LotComment { LotId = 1, UserId = "veklich99@mail.ru", Message = "Message2" } } };
            lotOperationsHandler.AddLot(lot1, "", "");
            lotOperationsHandler.AddLot(lot2, "", "");

            var resultLots = lotOperationsHandler.GetAllLots(l => l.LotComments).Where(l => l.LotComments.Where(c => c.Message == "Message2").Count() > 0);

            Assert.AreEqual(1, resultLots.Count());
            Assert.AreEqual("Message2", resultLots.ToList()[0].LotComments[0].Message);
        }

        [Test]
        public void ChangeLot_ChangedNameAndComments_NameChangedCommentsNotChanged()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Name1", LotComments = new List<LotComment> { new LotComment { Message = "Message1", LotId = 1, UserId = "veklich99@mail.ru" } } };
            lotOperationsHandler.AddLot(lot, "", "");

            var lotComment = new LotComment { Message = "Message2", LotId = 1, UserId = "veklich99@mail.ru" };
            var modifiedLot = lotOperationsHandler.GetLot(1, l => l.LotComments, l => l.LotPhotos, l => l.SellerUser);
            modifiedLot.LotComments.Add(lotComment); //List was initialized because of Automapper
            modifiedLot.Name = "Name2";
            lotOperationsHandler.ChangeLot(1, modifiedLot, "", "");

            var resultLot = lotOperationsHandler.GetLot(1, l => l.LotComments);
            Assert.AreEqual(1, lotOperationsHandler.GetAllLots().Count());
            Assert.AreEqual(1, resultLot.LotComments.Count());
            Assert.AreEqual("Name2", resultLot.Name);
            Assert.AreEqual("Message1", resultLot.LotComments[0].Message);
        }

        [Test]
        public void ChangeLot_InvalidLot_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot, "", "");

            var modifiedLot = lotOperationsHandler.GetLot(1, l => l.LotComments, l => l.LotPhotos, l => l.SellerUser);
            modifiedLot.Id = 2;

            Assert.Throws<ArgumentException>(() => lotOperationsHandler.ChangeLot(1, modifiedLot, "", ""));

            modifiedLot.Id = 1;
            modifiedLot.SellerUserId = "veklich99@gmail.com";
            Assert.Throws<ArgumentException>(() => lotOperationsHandler.ChangeLot(1, modifiedLot, "", ""));

            modifiedLot.SellerUserId = "veklich99@mail.ru";
            modifiedLot.BuyerUserId = "veklich99@mail.ru";
            Assert.Throws<ArgumentException>(() => lotOperationsHandler.ChangeLot(1, modifiedLot, "", ""));
        }

        [Test]
        public void DeleteLot_ValidId_DeletesInDB()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot, "", "");

            lotOperationsHandler.DeleteLot(1, "", "");

            Assert.AreEqual(0, lotOperationsHandler.GetAllLots().Count());
        }

        [Test]
        public void DeleteLot_InvalidId_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot, "", "");

            Assert.Throws<ArgumentException>(() => lotOperationsHandler.DeleteLot(2, "", ""));
        }

        [Test]
        public void PlaceBid_ValidInput_ChangesPriceAndBuyerUserId()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Name1" };
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
        public void PlaceBid_InvalidInput_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot, "", "");

            Assert.Throws<ArgumentException>(() => lotOperationsHandler.PlaceBid(1, "veklich98@gmail.com", 15));
            Assert.Throws<ArgumentException>(() => lotOperationsHandler.PlaceBid(3, "veklich99@mail.ru", 15));
        }
    }
}
