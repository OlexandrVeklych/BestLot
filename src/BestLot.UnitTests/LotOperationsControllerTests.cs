using BusinessLogicLayer.LogicHandlers;
using BusinessLogicLayer.Models;
using DataAccessLayer.Entities;
using DataAccessLayer.UnitOfWork;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests
{
    [TestFixture]
    public class LotOperationsControllerTests
    {
        private LotOperationsHandler lotOperationsHandler;
        private IUnitOfWork unitOfWork;

        [SetUp]
        public void SetUp()
        {
            unitOfWork = UnitTestDependencyResolver.Resolve();
            lotOperationsHandler = new LotOperationsHandler(unitOfWork);
            unitOfWork.UserAccounts.Add(new UserAccountInfoEntity { Name = "DefaultUser" });
            unitOfWork.SaveChanges();
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

            Assert.AreEqual(1, unitOfWork.Lots.GetAll().Count());
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
            var lot1 = new Lot { SellerUserId = 1, Name = "Name1", SellDate = DateTime.Now, StartDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { LotId = 1, UserId = 1, Message = "Message1" } } };
            var lot2 = new Lot { SellerUserId = 1, Name = "Name2", SellDate = DateTime.Now, StartDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { LotId = 1, UserId = 1, Message = "Message2" } } };
            lotOperationsHandler.AddLot(lot1);
            lotOperationsHandler.AddLot(lot2);

            var resultLot = lotOperationsHandler.GetLot(2, l => l.Comments, l => l.SellerUser);

            Assert.AreEqual("DefaultUser", resultLot.SellerUser.Name);
            Assert.AreEqual("Name2", resultLot.Name);
            Assert.AreEqual("Message2", resultLot.Comments[0].Message);
        }

        [Test]
        public void GetAllLots_DBContains1Elem_CountReturns1Elem()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now };

            lotOperationsHandler.AddLot(lot);

            Assert.AreEqual(1, lotOperationsHandler.GetAllLots().Count());
        }

        [Test]
        public void GetAllLots_WithInclude_ReturnsObjectWithInnerProperties()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { LotId = 1, UserId = 1, Message = "Message1" } } };
            lotOperationsHandler.AddLot(lot);

            var resultLot = lotOperationsHandler.GetAllLots(l => l.Comments).First();

            Assert.AreEqual("Message1", resultLot.Comments[0].Message);
        }

        [Test]
        public void GetAllLots_WithIncludeAndFilter_ReturnsFilteredObjectWithInnerProperties()
        {
            var lot1 = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { LotId = 1, UserId = 1, Message = "Message1" } } };
            var lot2 = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { LotId = 1, UserId = 1, Message = "Message2" } } };
            lotOperationsHandler.AddLot(lot1);
            lotOperationsHandler.AddLot(lot2);

            var resultLots = lotOperationsHandler.GetAllLots(l => l.Comments[0].Message == "Message2", l => l.Comments);

            Assert.AreEqual(1, resultLots.Count());
            Assert.AreEqual("Message2", resultLots.ToList()[0].Comments[0].Message);
        }

        [Test]
        public void ModifyLotWithoutMaping_ValidInput_ModifiesInDB()
        {
            var lot = new LotEntity { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Name = "Name1" };
            unitOfWork.Lots.Add(lot);
            unitOfWork.SaveChanges();

            var modifiedLot = unitOfWork.Lots.Get(1, l => l.Comments, l => l.LotPhotos, l => l.SellerUser);
            modifiedLot.Name = "Name2";
            unitOfWork.Lots.Modify(1, modifiedLot);
            unitOfWork.SaveChanges();

            Assert.AreEqual(1, unitOfWork.Lots.GetAll().Count());
            Assert.AreEqual("Name2", unitOfWork.Lots.Get(1).Name);
        }

        [Test]
        public void ModifyLotWithMaping_ValidInput_ModifiesInDB()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot);

            var modifiedLot = lotOperationsHandler.GetLot(1, l => l.Comments, l => l.LotPhotos, l => l.SellerUser);
            modifiedLot.Name = "Name2";
            lotOperationsHandler.ChangeLot(1, modifiedLot);

            var resultLot = lotOperationsHandler.GetLot(1);
            Assert.AreEqual(1, unitOfWork.Lots.GetAll().Count());
            Assert.AreEqual("Name2", resultLot.Name);
        }

        [Test]
        public void ModifyLotWithoutMaping_ChangedInnerList_ModifiesInDB()
        {
            var lot = new LotEntity { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Name = "Name1" };
            unitOfWork.Lots.Add(lot);
            unitOfWork.SaveChanges();

            var lotComment = new LotCommentEntity { Message = "Message1", LotId = 1, UserId = 1 };
            var modifiedLot = unitOfWork.Lots.Get(1, l => l.Comments, l => l.LotPhotos, l => l.SellerUser);
            modifiedLot.Comments = new List<LotCommentEntity> { lotComment }; //List wasn`t initialized before
            modifiedLot.Name = "Name2";
            unitOfWork.Lots.Modify(1, modifiedLot);
            unitOfWork.SaveChanges();

            var resultLot = unitOfWork.Lots.Get(1, l => l.Comments);
            Assert.AreEqual(1, unitOfWork.Lots.GetAll().Count());
            Assert.AreEqual(1, resultLot.Comments.Count());
            Assert.AreEqual("Name2", resultLot.Name);
            Assert.AreEqual("Message1", resultLot.Comments[0].Message);
        }

        [Test]
        public void ModifyLotWithMaping_ChangedInnerList_ModifiesInDB()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot);

            var lotComment = new LotComment { Message = "Message1", LotId = 1, UserId = 1 };
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
        public void DeleteLot_ValidInput_DeletesInDB()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot);

            lotOperationsHandler.DeleteLot(1);

            Assert.AreEqual(0, unitOfWork.Lots.GetAll().Count());
        }

        [Test]
        public void PlaceBet_ValidInput_ChangesPriceAndBuyerUserId()
        {
            var lot = new Lot { SellerUser = new UserAccountInfo { Name = "Seller" }, SellDate = DateTime.Now, StartDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot);

            lotOperationsHandler.PlaceBet(1, 1, 15);

            Assert.AreEqual(1, unitOfWork.Lots.GetAll().Count());
            Assert.AreEqual(2, unitOfWork.UserAccounts.GetAll().Count());
            Assert.AreEqual(15, unitOfWork.Lots.Get(1).Price);
            Assert.AreEqual(1, unitOfWork.Lots.Get(1).BuyerUserId);
            Assert.AreEqual("DefaultUser", unitOfWork.UserAccounts.Get(1).Name);
        }

        [Test]
        public void AddComment_ValidInput_AddedComment()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Name = "Name1" };
            lotOperationsHandler.AddLot(lot);
            unitOfWork.UserAccounts.Add(new UserAccountInfoEntity { Name = "Commenter" });
            unitOfWork.SaveChanges();

            var lotComment = new LotComment { Message = "Comment1", UserId = 2, LotId = 1 };
            lotOperationsHandler.AddComment(lotComment);

            Assert.AreEqual(1, unitOfWork.Lots.GetAll().Count());
            Assert.AreEqual(2, unitOfWork.UserAccounts.GetAll().Count());
            Assert.AreEqual("Comment1", unitOfWork.Lots.Get(1).Comments[0].Message);
            Assert.AreEqual("Commenter", unitOfWork.Lots.Get(1).Comments[0].User.Name);
            Assert.AreEqual("Comment1", unitOfWork.UserAccounts.Get(3).LotComments[0].Message);
            // Works :D
            Assert.AreEqual("Name1", unitOfWork.UserAccounts.Get(3).LotComments[0].Lot.Comments[0].User.LotComments[0].Lot.Name);

        }
    }
}
