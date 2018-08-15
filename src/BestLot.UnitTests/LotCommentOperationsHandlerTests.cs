using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestLot.BusinessLogicLayer.Exceptions;
using BestLot.BusinessLogicLayer.Interfaces;
using BestLot.BusinessLogicLayer.Models;
using BestLot.DataAccessLayer.UnitOfWork;
using NUnit.Framework;

namespace BestLot.UnitTests
{
    [TestFixture]
    public class LotCommentOperationsHandlerTests
    {
        private ILotOperationsHandler lotOperationsHandler;
        private ILotCommentOperationsHandler lotCommentOperationsHandler;
        private IUserAccountOperationsHandler userAccountOperationsHandler;
        private IUnitOfWork unitOfWork;

        [SetUp]
        public void SetUp()
        {
            unitOfWork = UnitTestDependencyResolver.ResolveUnitOfWork();
            lotOperationsHandler = UnitTestDependencyResolver.ResolveLotOperationsHandler(unitOfWork);
            userAccountOperationsHandler = UnitTestDependencyResolver.ResolveUserAccountOperationsHandler(unitOfWork);
            lotCommentOperationsHandler = UnitTestDependencyResolver.ResolveLotCommentOperationsHandler(unitOfWork);
            userAccountOperationsHandler.AddUserAccount(new UserAccountInfo { Name = "DefaultUser", Email = "veklich99@mail.ru" });
        }

        [TearDown]
        public void TearDown()
        {
            unitOfWork.RecreateDB();
            lotOperationsHandler.Dispose();
            userAccountOperationsHandler.Dispose();
            lotCommentOperationsHandler.Dispose();
        }

        [Test]
        public void AddComment_ValidInput_AddsComment()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", StartDate = DateTime.Now, SellDate = DateTime.Now.AddDays(1).AddDays(1), Name = "Name1" };
            lotOperationsHandler.AddLot(lot, "", "");

            var lotComment = new LotComment { Message = "Comment1", UserId = "veklich99@mail.ru", LotId = 1 };
            lotCommentOperationsHandler.AddComment(lotComment);
            var lotComment2 = new LotComment { Message = "Comment2", UserId = "veklich99@mail.ru", LotId = 1 };
            lotCommentOperationsHandler.AddComment(lotComment2);

            var resultLot = lotOperationsHandler.GetLot(1);
            resultLot.LotComments = lotCommentOperationsHandler.GetLotComments(1).ToList();
            var resultUser = userAccountOperationsHandler.GetUserAccount("veklich99@mail.ru");
            resultUser.LotComments = lotCommentOperationsHandler.GetUserComments(resultUser.Email).ToList();

            Assert.AreEqual(1, lotOperationsHandler.GetAllLots().Count());
            Assert.AreEqual(2, resultLot.LotComments.Count());
            Assert.AreEqual(2, resultUser.LotComments.Count());
            Assert.AreEqual("Comment1", resultLot.LotComments[0].Message);
            Assert.AreEqual("DefaultUser", resultLot.LotComments[0].User.Name);
            Assert.AreEqual("Comment2", resultLot.LotComments[1].Message);
            Assert.AreEqual("DefaultUser", resultLot.LotComments[1].User.Name);
        }

        [Test]
        public void AddComment_InvalidInput_ThrowsWrongIdException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", StartDate = DateTime.Now, SellDate = DateTime.Now.AddDays(1).AddDays(1), Name = "Name1" };
            lotOperationsHandler.AddLot(lot, "", "");
            userAccountOperationsHandler.AddUserAccount(new UserAccountInfo { Name = "Commenter", Email = "veklich99@gmail.com" });
            var invalidUserIdComment = new LotComment { Message = "Comment1", UserId = "veklich98@gmail.com", LotId = 1 };
            var invalidLotIdComment = new LotComment { Message = "Comment1", UserId = "veklich99@gmail.com", LotId = 2 };

            Assert.Throws<WrongIdException>(() => lotCommentOperationsHandler.AddComment(invalidUserIdComment));
            Assert.Throws<WrongIdException>(() => lotCommentOperationsHandler.AddComment(invalidLotIdComment));
        }
    }
}
