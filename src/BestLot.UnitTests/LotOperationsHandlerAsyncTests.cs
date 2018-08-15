using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BestLot.BusinessLogicLayer.Interfaces;
using BestLot.BusinessLogicLayer.Models;
using BestLot.DataAccessLayer.UnitOfWork;
using NUnit.Framework;

namespace BestLot.UnitTests
{
    [TestFixture]
    public class LotOperationsHandlerAsyncTests
    {
        private ILotOperationsHandler lotOperationsHandler;
        private IUserAccountOperationsHandler userAccountOperationsHandler;
        private ILotCommentOperationsHandler lotCommentOperationsHandler;
        private IUnitOfWork unitOfWork;

        [SetUp]
        public async Task SetUpAsync()
        {
            unitOfWork = UnitTestDependencyResolver.ResolveUnitOfWork();
            lotOperationsHandler = UnitTestDependencyResolver.ResolveLotOperationsHandler(unitOfWork);
            userAccountOperationsHandler = UnitTestDependencyResolver.ResolveUserAccountOperationsHandler(unitOfWork);
            lotCommentOperationsHandler = UnitTestDependencyResolver.ResolveLotCommentOperationsHandler(unitOfWork);
            await userAccountOperationsHandler.AddUserAccountAsync(new UserAccountInfo { Name = "DefaultUser", Email = "veklich99@mail.ru" });
        }

        [TearDown]
        public void TearDown()
        {
            unitOfWork.RecreateDB();
        }

        [Test]
        public async Task AddLotAsync_ValidInput_AddsLotToDB()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now };

            await lotOperationsHandler.AddLotAsync(lot, "", "");

            Assert.AreEqual(1, (await lotOperationsHandler.GetAllLotsAsync()).Count());
        }

        [Test]
        public void AddLotAsync_InvalidInput_ThrowsArgumentException()
        {
            var invalidEmailLot = new Lot { SellerUserId = "veklich99@gmail.com", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now };
            var invalidDatesLot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, StartDate = DateTime.Now.AddDays(1) };

            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.AddLotAsync(invalidEmailLot, "", ""));
            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.AddLotAsync(invalidDatesLot, "", ""));
        }

        [Test]
        public async Task GetLotAsync_GetWithId2_ReturnsCorrectElem()
        {
            var lot1 = new Lot { SellerUserId = "veklich99@mail.ru", Name = "Name1", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now };
            var lot2 = new Lot { SellerUserId = "veklich99@mail.ru", Name = "Name2", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now };
            await lotOperationsHandler.AddLotAsync(lot1, "", "");
            await lotOperationsHandler.AddLotAsync(lot2, "", "");

            var resultLot = await lotOperationsHandler.GetLotAsync(2);

            Assert.AreEqual("Name2", resultLot.Name);
        }

        [Test]
        public async Task GetLotAsync_InvalidId_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", Name = "Name1", SellDate = DateTime.Now.AddDays(1), LotComments = new List<LotComment> { new LotComment { LotId = 1, UserId = "veklich99@mail.ru", Message = "Message1" } } };
            await lotOperationsHandler.AddLotAsync(lot, "", "");

            Assert.ThrowsAsync<ArgumentException>(async () => await lotOperationsHandler.GetLotAsync(2));
        }

        [Test]
        public async Task GetAllLotsAsync_DBContains1Elem_CountReturns1Elem()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), StartDate = DateTime.Now };
            await lotOperationsHandler.AddLotAsync(lot, "", "");

            var resultLots = await lotOperationsHandler.GetAllLotsAsync();

            Assert.AreEqual(1, resultLots.Count());
        }

        [Test]
        public async Task ChangeLotAsync_ChangedNameAndComments_NameChangedCommentsNotChanged()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), Name = "Name1", LotComments = new List<LotComment> { new LotComment { Message = "Message1", LotId = 1, UserId = "veklich99@mail.ru" } } };
            await lotOperationsHandler.AddLotAsync(lot, "", "");

            var lotComment = new LotComment { Message = "Message2", LotId = 1, UserId = "veklich99@mail.ru" };
            var modifiedLot = await lotOperationsHandler.GetLotAsync(1);
            modifiedLot.LotComments = new List<LotComment> { lotComment };
            modifiedLot.Name = "Name2";
            await lotOperationsHandler.ChangeLotAsync(1, modifiedLot, "", "");

            var resultLot = await lotOperationsHandler.GetLotAsync(1);
            resultLot.LotComments = (await lotCommentOperationsHandler.GetLotCommentsAsync(1)).ToList();

            Assert.AreEqual(1, lotOperationsHandler.GetAllLots().Count());
            Assert.AreEqual(1, resultLot.LotComments.Count());
            Assert.AreEqual("Name2", resultLot.Name);
            Assert.AreEqual("Message1", resultLot.LotComments[0].Message);
        }

        [Test]
        public async Task ChangeLotAsync_LotNotExits_AddsLot()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), Name = "Name1" };
            await lotOperationsHandler.ChangeLotAsync(1, lot, "", "");

            Assert.AreEqual(1, (await lotOperationsHandler.GetAllLotsAsync()).Count());
        }

        [Test]
        public async Task ChangeLotAsync_InvalidLot_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), Name = "Name1" };
            await lotOperationsHandler.AddLotAsync(lot, "", "");

            var modifiedLot = await lotOperationsHandler.GetLotAsync(1);

            //Change id - not allowed
            modifiedLot.Id = 2;
            Assert.ThrowsAsync<ArgumentException>(async () => await lotOperationsHandler.ChangeLotAsync(1, modifiedLot, "", ""));

            //Reset id, change seller user - not allowed
            modifiedLot.Id = 1;
            modifiedLot.SellerUserId = "veklich99@gmail.com";
            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.ChangeLotAsync(1, modifiedLot, "", ""));

            //Reset seller user, change buyer user - not allowed
            modifiedLot.SellerUserId = "veklich99@mail.ru";
            modifiedLot.BuyerUserId = "veklich99@mail.ru";
            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.ChangeLotAsync(1, modifiedLot, "", ""));
        }

        [Test]
        public async Task DeleteLotAsync_ValidId_DeletesInDB()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), Name = "Name1" };
            await lotOperationsHandler.AddLotAsync(lot, "", "");
            await lotCommentOperationsHandler.AddCommentAsync(new LotComment { Message = "Comment1", LotId = 1, UserId = "veklich99@mail.ru" });
            await lotOperationsHandler.DeleteLotAsync(1, "", "");

            Assert.AreEqual(0, (await lotOperationsHandler.GetAllLotsAsync()).Count());
            Assert.AreEqual(0, (await unitOfWork.LotComments.GetAllAsync()).Count());
        }

        [Test]
        public async Task DeleteLotAsync_InvalidId_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), Name = "Name1" };
            await lotOperationsHandler.AddLotAsync(lot, "", "");

            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.DeleteLotAsync(2, "", ""));
        }

        [Test]
        public async Task PlaceBidAsync_ValidInput_ChangesPriceAndBuyerUserId()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), Name = "Name1" };
            var user = new UserAccountInfo { Name = "SecondUser", Email = "veklich98@mail.ru" };
            await lotOperationsHandler.AddLotAsync(lot, "", "");
            await userAccountOperationsHandler.AddUserAccountAsync(user);

            await lotOperationsHandler.PlaceBidAsync(1, "veklich98@mail.ru", 15);

            var resultLot = await lotOperationsHandler.GetLotAsync(1);
            Assert.AreEqual(1, (await lotOperationsHandler.GetAllLotsAsync()).Count());
            Assert.AreEqual(15, resultLot.Price);
            Assert.AreEqual("veklich98@mail.ru", resultLot.BuyerUserId);
        }

        [Test]
        public async Task PlaceBidAsync_InvalidInput_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), Name = "Name1" };
            await lotOperationsHandler.AddLotAsync(lot, "", "");

            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.PlaceBidAsync(1, "veklich98@mail.ru", 15));//price < price + step
            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.PlaceBidAsync(1, "veklich99@mail.ru", 15));//Bid for own lot
            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.PlaceBidAsync(1, "veklich98@gmail.com", 15));//Invalid email
            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.PlaceBidAsync(3, "veklich99@mail.ru", 15));//Invalid lot id
        }

        [Test]
        public async Task PlaceBidAsync_RelativeBidplacer_ChangesPriceAndBuyerUserId()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), Name = "Name1", BidPlacer = "Relative" };
            var user = new UserAccountInfo { Name = "SecondUser", Email = "veklich98@mail.ru" };
            await lotOperationsHandler.AddLotAsync(lot, "", "");
            await userAccountOperationsHandler.AddUserAccountAsync(user);

            await lotOperationsHandler.PlaceBidAsync(1, "veklich98@mail.ru", 15);

            var resultLot = await lotOperationsHandler.GetLotAsync(1);

            Assert.AreEqual(1, (await lotOperationsHandler.GetAllLotsAsync()).Count());
            Assert.AreEqual(15, resultLot.Price);
            Assert.AreEqual("veklich98@mail.ru", resultLot.BuyerUserId);
            Assert.AreEqual(-1, lot.StartDate.CompareTo(resultLot.StartDate));//Start date moved forward
            Assert.AreEqual(-1, lot.SellDate.CompareTo(resultLot.SellDate));//Sell date moved forward
        }

        [Test]
        public async Task PlaceBidAsync_RelativeBidplacer_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now.AddDays(1), Name = "Name1", BidPlacer = "Relative" };
            await lotOperationsHandler.AddLotAsync(lot, "", "");

            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.PlaceBidAsync(1, "veklich98@mail.ru", 15));//price < price + step
            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.PlaceBidAsync(1, "veklich99@mail.ru", 15));//Bid for own lot
            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.PlaceBidAsync(1, "veklich98@gmail.com", 15));//Invalid email
            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.PlaceBidAsync(3, "veklich99@mail.ru", 15));//Invalid lot id
        }
    }
}
