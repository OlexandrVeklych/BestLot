using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using DataAccessLayer.Entities;
using DataAccessLayer.UnitOfWork;
using BusinessLogicLayer;
using BusinessLogicLayer.Models;

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

        [Test]
        public void AddLot_ValidInput_AddsLotToDB()
        {
            lotOperationsController.AddLot(new Lot());

            Assert.AreEqual(1, unitOfWork.Lots.GetAll());
        }

        [Test]
        public void GetAllLots_DBContains1Elem_CountReturns1Elem()
        {
            unitOfWork.Lots.Add(new LotEntity());

            Assert.AreEqual(1, lotOperationsController.GetAllLots());
        }

        [Test]
        public void ModifyLotWithoutMaping_ValidInput_ModifiesInDB()
        {
            unitOfWork.Lots.Add(new LotEntity { Name = "Name1" });

            unitOfWork.Lots.Modify(1, new LotEntity { Id = 1, Name = "Name2" });

            Assert.AreEqual(1, unitOfWork.Lots.GetAll());
            Assert.AreEqual("Name2", unitOfWork.Lots.Get(1).Name);
        }

        [Test]
        public void ModifyLotWithMaping_ValidInput_ModifiesInDB()
        {
            lotOperationsController.AddLot(new Lot { Name = "Name1" });

            lotOperationsController.ChangeLot(1, new Lot { Id = 1, Name = "Name2" });

            Assert.AreEqual(1, unitOfWork.Lots.GetAll());
            Assert.AreEqual("Name2", unitOfWork.Lots.Get(1).Name);
        }
    }
}
