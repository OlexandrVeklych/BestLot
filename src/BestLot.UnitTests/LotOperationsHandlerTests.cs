using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogicLayer.LogicHandlers;
using BusinessLogicLayer.Models;
using DataAccessLayer.UnitOfWork;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class LotOperationsHandlerTests
    {
        private LotOperationsHandler lotOperationsHandler;
        private UserAccountOperationsHandler userAccountOperationsHandler;
        private IUnitOfWork unitOfWork;

        [SetUp]
        public void SetUp()
        {
            unitOfWork = UnitTestDependencyResolver.Resolve();
            lotOperationsHandler = new LotOperationsHandler(unitOfWork);
            userAccountOperationsHandler = new UserAccountOperationsHandler(unitOfWork);
            userAccountOperationsHandler.AddUserAccount(new UserAccountInfo { Name = "DefaultUser" });
        }

        [TearDown]
        public void TearDown()
        {
            unitOfWork.RecreateDB();
        }

        [Test]
        public void AddLot_ValidInput_AddsLotToDB()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now };

            lotOperationsHandler.AddLot(lot);

            Assert.AreEqual(1, lotOperationsHandler.GetAllLots().Count());
        }

        [Test]
        public void AddLot_InvalidInput_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = 2, SellDate = DateTime.Now, StartDate = DateTime.Now };

            Assert.Throws<ArgumentException>(() => lotOperationsHandler.AddLot(lot));
        }

        [Test]
        public void GetLotWithoutInclude_GetWithId2_ReturnsCorrectElem()
        {
            var lot1 = new Lot { SellerUserId = 1, Name = "Name1", SellDate = DateTime.Now, StartDate = DateTime.Now };
            var lot2 = new Lot { SellerUserId = 1, Name = "Name2", SellDate = DateTime.Now, StartDate = DateTime.Now };
            lotOperationsHandler.AddLot(lot1);
            lotOperationsHandler.AddLot(lot2);

            var resultLot = lotOperationsHandler.GetLot(2);

            Assert.AreEqual("Name2", resultLot.Name);
        }

        [Test]
        public void GetLotWithInclude_GetWithId2_ReturnsCorrectElem()
        {
            var lot1 = new Lot { SellerUserId = 1, Name = "Name1", SellDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { LotId = 1, UserId = 1, Message = "Message1" } } };
            var lot2 = new Lot { SellerUserId = 1, Name = "Name2", SellDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { LotId = 1, UserId = 1, Message = "Message2" } } };
            lotOperationsHandler.AddLot(lot1);
            lotOperationsHandler.AddLot(lot2);

            var resultLot = lotOperationsHandler.GetLot(2, l => l.Comments, l => l.SellerUser);

            Assert.AreEqual("DefaultUser", resultLot.SellerUser.Name);
            Assert.AreEqual("Name2", resultLot.Name);
            Assert.AreEqual("Message2", resultLot.Comments[0].Message);
        }

        [Test]
        public void GetLot_InvalidId_ThrowsArgumentException()
        {
            var lot1 = new Lot { SellerUserId = 1, Name = "Name1", SellDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { LotId = 1, UserId = 1, Message = "Message1" } } };
            lotOperationsHandler.AddLot(lot1);

            Assert.Throws<ArgumentException>(() => lotOperationsHandler.GetLot(2));
        }

        [Test]
        public void GetAllLots_DBContains1Elem_CountReturns1Elem()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now };
            lotOperationsHandler.AddLot(lot);

            var resultLots = lotOperationsHandler.GetAllLots();

            Assert.AreEqual(1, resultLots.Count());
        }

        [Test]
        public void GetAllLots_WithInclude_ReturnsObjectWithInnerProperties()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { LotId = 1, UserId = 1, Message = "Message1" } } };
            lotOperationsHandler.AddLot(lot);

            var resultLots = lotOperationsHandler.GetAllLots(l => l.Comments);

            Assert.AreEqual("Message1", resultLots.ToList()[0].Comments[0].Message);
        }

        [Test]
        public void GetAllLots_WithIncludeAndFilter_ReturnsFilteredObjectWithInnerProperties()
        {
            var lot1 = new Lot { SellerUserId = 1, SellDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { LotId = 1, UserId = 1, Message = "Message1" } } };
            var lot2 = new Lot { SellerUserId = 1, SellDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { LotId = 1, UserId = 1, Message = "Message2" } } };
            lotOperationsHandler.AddLot(lot1);
            lotOperationsHandler.AddLot(lot2);

            var resultLots = lotOperationsHandler.GetAllLots(l => l.Comments).Where(l => l.Comments.Where(c => c.Message == "Message2").Count() > 0);

            Assert.AreEqual(1, resultLots.Count());
            Assert.AreEqual("Message2", resultLots.ToList()[0].Comments[0].Message);
        }

        [Test]
        public void ChangeLot_ChangedNameAndComments_NameChangedCommentsNotChanged()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, Name = "Name1", Comments = new List<LotComment> { new LotComment { Message = "Message1", LotId = 1, UserId = 1 } } };
            lotOperationsHandler.AddLot(lot);

            var lotComment = new LotComment { Message = "Message2", LotId = 1, UserId = 1 };
            var modifiedLot = lotOperationsHandler.GetLot(1, l => l.Comments, l => l.LotPhotos, l => l.SellerUser);
            modifiedLot.Comments.Add(lotComment); //List was initialized because of Automapper
            modifiedLot.Name = "Name2";          
            lotOperationsHandler.ChangeLot(1, modifiedLot);

            var resultLot = lotOperationsHandler.GetLot(1, l => l.Comments);
            Assert.AreEqual(1, lotOperationsHandler.GetAllLots().Count());
            Assert.AreEqual(1, resultLot.Comments.Count());
            Assert.AreEqual("Name2", resultLot.Name);
            Assert.AreEqual("Message1", resultLot.Comments[0].Message);
        }

        [Test]
        public void ChangeLot_InvalidLot_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot);

            var modifiedLot = lotOperationsHandler.GetLot(1, l => l.Comments, l => l.LotPhotos, l => l.SellerUser);
            modifiedLot.Id = 2;

            Assert.Throws<ArgumentException>(() => lotOperationsHandler.ChangeLot(1, modifiedLot));

            modifiedLot.Id = 1;
            modifiedLot.SellerUserId = 2;
            Assert.Throws<ArgumentException>(() => lotOperationsHandler.ChangeLot(1, modifiedLot));

            modifiedLot.SellerUserId = 1;
            modifiedLot.BuyerUserId = 1;
            Assert.Throws<ArgumentException>(() => lotOperationsHandler.ChangeLot(1, modifiedLot));
        }

        [Test]
        public void DeleteLot_ValidId_DeletesInDB()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot);

            lotOperationsHandler.DeleteLot(1);

            Assert.AreEqual(0, lotOperationsHandler.GetAllLots().Count());
        }

        [Test]
        public void DeleteLot_InvalidId_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot);

            Assert.Throws<ArgumentException>(() => lotOperationsHandler.DeleteLot(2));
        }

        [Test]
        public void PlaceBet_ValidInput_ChangesPriceAndBuyerUserId()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot);

            lotOperationsHandler.PlaceBet(1, 1, 15);

            var resultLot = lotOperationsHandler.GetLot(1);
            Assert.AreEqual(1, lotOperationsHandler.GetAllLots().Count());
            Assert.AreEqual(15, resultLot.Price);
            Assert.AreEqual(1, resultLot.BuyerUserId);
            Assert.AreEqual("DefaultUser", userAccountOperationsHandler.GetUserAccount(1).Name);
        }

        [Test]
        public void PlaceBet_InvalidInput_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot);

            Assert.Throws<ArgumentException>(() => lotOperationsHandler.PlaceBet(3, 1, 15));
            Assert.Throws<ArgumentException>(() => lotOperationsHandler.PlaceBet(1, 3, 15));
        }

        [Test]
        public void AddComment_ValidInput_AddsComment()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot);
            userAccountOperationsHandler.AddUserAccount(new UserAccountInfo { Name = "Commenter" });

            var lotComment = new LotComment { Message = "Comment1", UserId = 1, LotId = 1 };
            lotOperationsHandler.AddComment(lotComment);
            var lotComment2 = new LotComment { Message = "Comment2", UserId = 1, LotId = 1 };
            lotOperationsHandler.AddComment(lotComment2);

            var resultLot = lotOperationsHandler.GetLot(1, l => l.Comments, l => l.SellerUser);

            Assert.AreEqual(1, lotOperationsHandler.GetAllLots().Count());
            Assert.AreEqual(2, userAccountOperationsHandler.GetAllUserAccounts().Count());
            Assert.AreEqual(2, resultLot.Comments.Count());
            Assert.AreEqual(2, userAccountOperationsHandler.GetUserAccount(1).LotComments.Count());
            Assert.AreEqual("Comment1", resultLot.Comments[0].Message);
            Assert.AreEqual("DefaultUser", resultLot.Comments[0].User.Name);
            Assert.AreEqual("Comment2", resultLot.Comments[1].Message);
            Assert.AreEqual("DefaultUser", resultLot.Comments[1].User.Name);
            // Works :D
            Assert.AreEqual("Name1", userAccountOperationsHandler.GetUserAccount(1).LotComments[0].Lot.Comments[0].User.LotComments[0].Lot.Name);
        }

        [Test]
        public void AddComment_InvalidInput_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot);
            userAccountOperationsHandler.AddUserAccount(new UserAccountInfo { Name = "Commenter" });
            var invalidUserIdComment = new LotComment { Message = "Comment1", UserId = 3, LotId = 1 };
            var invalidLotIdComment = new LotComment { Message = "Comment1", UserId = 2, LotId = 2 };

            Assert.Throws<ArgumentException>(() => lotOperationsHandler.AddComment(invalidUserIdComment));
            Assert.Throws<ArgumentException>(() => lotOperationsHandler.AddComment(invalidLotIdComment));
        }
    }
}
