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
        private IUnitOfWork unitOfWork;

        [SetUp]
        public async Task SetUpAsync()
        {
            unitOfWork = UnitTestDependencyResolver.ResolveUnitOfWork();
            lotOperationsHandler = UnitTestDependencyResolver.ResloveLotOperationsHandler(unitOfWork);
            userAccountOperationsHandler = UnitTestDependencyResolver.ResloveUserAccountOperationsHandler(unitOfWork);
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
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, StartDate = DateTime.Now };

            await lotOperationsHandler.AddLotAsync(lot);

            Assert.AreEqual(1, (await lotOperationsHandler.GetAllLotsAsync()).Count());
        }

        [Test]
        public void AddLotAsync_InvalidInput_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = "veklich99@gmail.com", SellDate = DateTime.Now, StartDate = DateTime.Now };

            Assert.ThrowsAsync<ArgumentException>(async () => await lotOperationsHandler.AddLotAsync(lot));
        }

        [Test]
        public async Task GetLotAsync_GetWithId2WithoutInclude_ReturnsCorrectElem()
        {
            var lot1 = new Lot { SellerUserId = "veklich99@mail.ru", Name = "Name1", SellDate = DateTime.Now, StartDate = DateTime.Now };
            var lot2 = new Lot { SellerUserId = "veklich99@mail.ru", Name = "Name2", SellDate = DateTime.Now, StartDate = DateTime.Now };
            await lotOperationsHandler.AddLotAsync(lot1);
            await lotOperationsHandler.AddLotAsync(lot2);

            var resultLot = await lotOperationsHandler.GetLotAsync(2);

            Assert.AreEqual("Name2", resultLot.Name);
        }

        [Test]
        public async Task GetLotAsync_GetWithId2WithInclude_ReturnsCorrectElem()
        {
            var lot1 = new Lot { SellerUserId = "veklich99@mail.ru", Name = "Name1", SellDate = DateTime.Now, LotComments = new List<LotComment> { new LotComment { LotId = 1, UserId = "veklich99@mail.ru", Message = "Message1" } } };
            var lot2 = new Lot { SellerUserId = "veklich99@mail.ru", Name = "Name2", SellDate = DateTime.Now, LotComments = new List<LotComment> { new LotComment { LotId = 1, UserId = "veklich99@mail.ru", Message = "Message2" } } };
            await lotOperationsHandler.AddLotAsync(lot1);
            await lotOperationsHandler.AddLotAsync(lot2);

            var resultLot = await lotOperationsHandler.GetLotAsync(2, l => l.LotComments, l => l.SellerUser);

            Assert.AreEqual("DefaultUser", resultLot.SellerUser.Name);
            Assert.AreEqual("Name2", resultLot.Name);
            Assert.AreEqual("Message2", resultLot.LotComments[0].Message);
        }

        [Test]
        public async Task GetLotAsync_InvalidId_ThrowsArgumentException()
        {
            var lot1 = new Lot { SellerUserId = "veklich99@mail.ru", Name = "Name1", SellDate = DateTime.Now, LotComments = new List<LotComment> { new LotComment { LotId = 1, UserId = "veklich99@mail.ru", Message = "Message1" } } };
            await lotOperationsHandler.AddLotAsync(lot1);

            Assert.ThrowsAsync<ArgumentException>(async () => await lotOperationsHandler.GetLotAsync(2));
        }

        [Test]
        public async Task GetAllLotsAsync_DBContains1Elem_CountReturns1Elem()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, StartDate = DateTime.Now };
            await lotOperationsHandler.AddLotAsync(lot);

            var resultLots = await lotOperationsHandler.GetAllLotsAsync();

            Assert.AreEqual(1, resultLots.Count());
        }

        [Test]
        public async Task GetAllLotsAsync_WithInclude_ReturnsObjectWithInnerProperties()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, LotComments = new List<LotComment> { new LotComment { LotId = 1, UserId = "veklich99@mail.ru", Message = "Message1" } } };
            await lotOperationsHandler.AddLotAsync(lot);

            var resultLots = await lotOperationsHandler.GetAllLotsAsync(l => l.LotComments);

            Assert.AreEqual("Message1", resultLots.ToList()[0].LotComments[0].Message);
        }

        [Test]
        public async Task GetAllLotsAsync_WithIncludeAndFilter_ReturnsFilteredObjectWithInnerProperties()
        {
            var lot1 = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, LotComments = new List<LotComment> { new LotComment { LotId = 1, UserId = "veklich99@mail.ru", Message = "Message1" } } };
            var lot2 = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, LotComments = new List<LotComment> { new LotComment { LotId = 1, UserId = "veklich99@mail.ru", Message = "Message2" } } };
            await lotOperationsHandler.AddLotAsync(lot1);
            await lotOperationsHandler.AddLotAsync(lot2);

            var resultLots = (await lotOperationsHandler.GetAllLotsAsync(l => l.LotComments)).Where(l => l.LotComments.Where(c => c.Message == "Message2").Count() > 0);

            Assert.AreEqual(1, resultLots.Count());
            Assert.AreEqual("Message2", resultLots.ToList()[0].LotComments[0].Message);
        }

        [Test]
        public async Task ChangeLotAsync_ChangedNameAndComments_NameChangedCommentsNotChanged()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Name1", LotComments = new List<LotComment> { new LotComment { Message = "Message1", LotId = 1, UserId = "veklich99@mail.ru" } } };
            await lotOperationsHandler.AddLotAsync(lot);

            var lotComment = new LotComment { Message = "Message2", LotId = 1, UserId = "veklich99@mail.ru" };
            var modifiedLot = await lotOperationsHandler.GetLotAsync(1, l => l.LotComments, l => l.LotPhotos, l => l.SellerUser);
            modifiedLot.LotComments.Add(lotComment); //List was initialized because of Automapper
            modifiedLot.Name = "Name2";
            await lotOperationsHandler.ChangeLotAsync(1, modifiedLot);

            var resultLot = await lotOperationsHandler.GetLotAsync(1, l => l.LotComments);
            Assert.AreEqual(1, lotOperationsHandler.GetAllLots().Count());
            Assert.AreEqual(1, resultLot.LotComments.Count());
            Assert.AreEqual("Name2", resultLot.Name);
            Assert.AreEqual("Message1", resultLot.LotComments[0].Message);
        }

        [Test]
        public async Task ChangeLotAsync_InvalidLot_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Name1" };
            await lotOperationsHandler.AddLotAsync(lot);

            var modifiedLot = await lotOperationsHandler.GetLotAsync(1, l => l.LotComments, l => l.LotPhotos, l => l.SellerUser);
            modifiedLot.Id = 2;

            Assert.ThrowsAsync<ArgumentException>(async () => await lotOperationsHandler.ChangeLotAsync(1, modifiedLot));

            modifiedLot.Id = 1;
            modifiedLot.SellerUserId = "veklich99@gmail.com";
            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.ChangeLotAsync(1, modifiedLot));

            modifiedLot.SellerUserId = "veklich99@mail.ru";
            modifiedLot.BuyerUserId = "veklich99@mail.ru";
            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.ChangeLotAsync(1, modifiedLot));
        }

        [Test]
        public async Task DeleteLotAsync_ValidId_DeletesInDB()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Name1" };
            await lotOperationsHandler.AddLotAsync(lot);

            await lotOperationsHandler.DeleteLotAsync(1);

            Assert.AreEqual(0, (await lotOperationsHandler.GetAllLotsAsync()).Count());
        }

        [Test]
        public async Task DeleteLotAsync_InvalidId_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Name1" };
            await lotOperationsHandler.AddLotAsync(lot);

            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.DeleteLotAsync(2));
        }

        [Test]
        public async Task PlaceBetAsync_ValidInput_ChangesPriceAndBuyerUserId()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Name1" };
            await lotOperationsHandler.AddLotAsync(lot);

            await lotOperationsHandler.PlaceBetAsync("veklich99@mail.ru", 1, 15);

            var resultLot = await lotOperationsHandler.GetLotAsync(1);
            Assert.AreEqual(1, (await lotOperationsHandler.GetAllLotsAsync()).Count());
            Assert.AreEqual(15, resultLot.Price);
            Assert.AreEqual("veklich99@mail.ru", resultLot.BuyerUserId);
            Assert.AreEqual("DefaultUser", (await userAccountOperationsHandler.GetUserAccountAsync("veklich99@mail.ru")).Name);
        }

        [Test]
        public async Task PlaceBetAsync_InvalidInput_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Name1" };
            await lotOperationsHandler.AddLotAsync(lot);

            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.PlaceBetAsync("veklich98@gmail.com", 1, 15));
            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.PlaceBetAsync("veklich99@mail.ru", 3, 15));
        }

        [Test]
        public async Task AddCommentAsync_ValidInput_AddsComment()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Name1" };
            await lotOperationsHandler.AddLotAsync(lot);

            var lotComment = new LotComment { Message = "Comment1", UserId = "veklich99@mail.ru", LotId = 1 };
            await lotOperationsHandler.AddCommentAsync(lotComment);
            var lotComment2 = new LotComment { Message = "Comment2", UserId = "veklich99@mail.ru", LotId = 1 };
            await lotOperationsHandler.AddCommentAsync(lotComment2);

            var resultLot = await lotOperationsHandler.GetLotAsync(1, l => l.LotComments, l => l.SellerUser);

            Assert.AreEqual(1, (await lotOperationsHandler.GetAllLotsAsync()).Count());
            Assert.AreEqual(2, resultLot.LotComments.Count());
            Assert.AreEqual(2, (await userAccountOperationsHandler.GetUserAccountAsync("veklich99@mail.ru")).LotComments.Count());
            Assert.AreEqual("Comment1", resultLot.LotComments[0].Message);
            Assert.AreEqual("DefaultUser", resultLot.LotComments[0].User.Name);
            Assert.AreEqual("Comment2", resultLot.LotComments[1].Message);
            Assert.AreEqual("DefaultUser", resultLot.LotComments[1].User.Name);
            // Works :D
            Assert.AreEqual("Name1", (await userAccountOperationsHandler.GetUserAccountAsync("veklich99@mail.ru")).LotComments[0].Lot.LotComments[0].User.LotComments[0].Lot.Name);
        }

        [Test]
        public async Task AddCommentAsync_InvalidInput_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Name1" };
            await lotOperationsHandler.AddLotAsync(lot);
            await userAccountOperationsHandler.AddUserAccountAsync(new UserAccountInfo { Name = "Commenter", Email = "veklich99@gmail.com" });
            var invalidUserIdComment = new LotComment { Message = "Comment1", UserId = "veklich98@gmail.com", LotId = 1 };
            var invalidLotIdComment = new LotComment { Message = "Comment1", UserId = "veklich99@gmail.com", LotId = 2 };

            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.AddCommentAsync(invalidUserIdComment));
            Assert.ThrowsAsync<ArgumentException>(() => lotOperationsHandler.AddCommentAsync(invalidLotIdComment));
        }
    }
}
