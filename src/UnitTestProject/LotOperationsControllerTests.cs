using BusinessLogicLayer;
using BusinessLogicLayer.Models;
using DataAccessLayer.Entities;
using DataAccessLayer.UnitOfWork;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestProject
{
    [TestFixture]
    public class LotOperationsControllerTests
    {
        private LotOperationsController lotOperationsController;
        private IUnitOfWork unitOfWork;

        [SetUp]
        public void SetUp()
        {
            unitOfWork = UnitTestDependencyResolver.Resolve();
            lotOperationsController = new LotOperationsController(unitOfWork);
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

            lotOperationsController.AddLot(lot);

            Assert.AreEqual(1, unitOfWork.Lots.GetAll().Count());
        }

        [Test]
        public void GetLotWithoutInclude_GetWithId2_ReturnsCorrectElem()
        {
            var lot1 = new Lot { SellerUserId = 1, Name = "Name1", SellDate = DateTime.Now, StartDate = DateTime.Now };
            var lot2 = new Lot { SellerUserId = 1, Name = "Name2", SellDate = DateTime.Now, StartDate = DateTime.Now };
            lotOperationsController.AddLot(lot1);
            lotOperationsController.AddLot(lot2);

            var lot = lotOperationsController.GetLot(2);

            Assert.AreEqual("Name2", lot.Name);
        }

        [Test]
        public void GetLotWithInclude_GetWithId2_ReturnsCorrectElem()
        {
            var lot1 = new Lot { SellerUserId = 1, Name = "Name1", SellDate = DateTime.Now, StartDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { Message = "Message1" } } };
            var lot2 = new Lot { SellerUserId = 1, Name = "Name2", SellDate = DateTime.Now, StartDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { Message = "Message2" } } };
            lotOperationsController.AddLot(lot1);
            lotOperationsController.AddLot(lot2);

            var lot = lotOperationsController.GetLot(2, l => l.Comments);

            Assert.AreEqual("Name2", lot.Name);
            Assert.AreEqual("Message2", lot.Comments[0].Message);
        }

        [Test]
        public void GetAllLots_DBContains1Elem_CountReturns1Elem()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now };

            lotOperationsController.AddLot(lot);

            Assert.AreEqual(1, lotOperationsController.GetAllLots().Count());
        }

        [Test]
        public void GetAllLots_WithInclude_ReturnsObjectWithInnerProperties()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { Message = "Message1" } } };
            lotOperationsController.AddLot(lot);

            var resultLot = lotOperationsController.GetAllLots(l => l.Comments).First();

            Assert.AreEqual("Message1", lot.Comments[0].Message);
        }

        [Test]
        public void GetAllLots_WithIncludeAndFilter_ReturnsFilteredObjectWithInnerProperties()
        {
            var lot1 = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { Message = "Message1" } } };
            var lot2 = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Comments = new List<LotComment> { new LotComment { Message = "Message2" } } };
            lotOperationsController.AddLot(lot1);
            lotOperationsController.AddLot(lot2);

            var resultLots = lotOperationsController.GetAllLots(l => l.Comments[0].Message == "Message2", l => l.Comments);

            Assert.AreEqual(1, resultLots.Count());
            Assert.AreEqual("Message2", resultLots.ToList()[0].Comments[0].Message);
        }

        [Test]
        public void ModifyLotWithoutMaping_ValidInput_ModifiesInDB()
        {
            var lot = new LotEntity { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Name = "Name1" };
            unitOfWork.Lots.Add(lot);
            unitOfWork.SaveChanges();
            var modifiedLot = new LotEntity { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Id = 1, Name = "Name2" };

            unitOfWork.Lots.Modify(1, modifiedLot);
            unitOfWork.SaveChanges();

            Assert.AreEqual(1, unitOfWork.Lots.GetAll().Count());
            Assert.AreEqual("Name2", unitOfWork.Lots.Get(1).Name);
        }

        [Test]
        public void ModifyLotWithMaping_ValidInput_ModifiesInDB()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Name = "Name1" };
            lotOperationsController.AddLot(lot);
            var modifiedLot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Id = 1, Name = "Name2" };

            lotOperationsController.ChangeLot(1, modifiedLot);

            Assert.AreEqual(1, unitOfWork.Lots.GetAll().Count());
            Assert.AreEqual("Name2", unitOfWork.Lots.Get(1).Name);
        }

        [Test]
        public void ModifyLotWithoutMaping_ChangedInnerList_ModifiesInDB()
        {
            var lot = new LotEntity { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Name = "Name1" };
            unitOfWork.Lots.Add(lot);
            unitOfWork.SaveChanges();
            var lotComment = new LotCommentEntity { Message = "Message1" };
            var modifiedLot = new LotEntity { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Id = 1, Name = "Name2", Comments = new List<LotCommentEntity> { lotComment } };
            lotComment.Lot = modifiedLot;
            lotComment.LotId = modifiedLot.Id;

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
            lotOperationsController.AddLot(lot);


            var lotComment = new LotComment { Message = "Message1" };
            var modifiedLot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Id = 1, Name = "Name2", Comments = new List<LotComment> { lotComment } };
            lotComment.Lot = modifiedLot;
            lotComment.LotId = modifiedLot.Id;

            lotOperationsController.ChangeLot(1, modifiedLot);

            var resultLot = lotOperationsController.GetLot(1, l => l.Comments);
            Assert.AreEqual(1, lotOperationsController.GetAllLots().Count());
            Assert.AreEqual(1, resultLot.Comments.Count());
            Assert.AreEqual("Name2", resultLot.Name);
            Assert.AreEqual("Message1", resultLot.Comments[0].Message);
        }

        [Test]
        public void DeleteLot_ValidInput_DeletesInDB()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Name = "Name1" };
            lotOperationsController.AddLot(lot);

            lotOperationsController.DeleteLot(1);

            Assert.AreEqual(0, unitOfWork.Lots.GetAll().Count());
        }

        [Test]
        public void PlaceBet_ValidInput_ChangesPriceAndBuyerUserId()
        {
            var lot = new Lot { SellerUserId = 1, SellDate = DateTime.Now, StartDate = DateTime.Now, Name = "Name1" };
            lotOperationsController.AddLot(lot);

            lotOperationsController.PlaceBet(1, 1, 15);

            Assert.AreEqual(1, unitOfWork.Lots.GetAll().Count());
            Assert.AreEqual(15, unitOfWork.Lots.Get(1).Price);
            Assert.AreEqual(1, unitOfWork.Lots.Get(1).BuyerUserId);
        } //Write tests with exceptions (Wrong id ...)
    }
}
