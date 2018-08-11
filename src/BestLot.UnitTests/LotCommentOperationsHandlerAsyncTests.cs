using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestLot.BusinessLogicLayer.Interfaces;
using BestLot.BusinessLogicLayer.Models;
using BestLot.DataAccessLayer.UnitOfWork;
using NUnit.Framework;

namespace BestLot.UnitTests
{
    [TestFixture]
    public class LotCommentOperationsHandlerAsyncTests
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
        }

        [Test]
        public async Task AddCommentAsync_ValidInput_AddsComment()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Name1" };
            await lotOperationsHandler.AddLotAsync(lot, "", "");

            var lotComment = new LotComment { Message = "Comment1", UserId = "veklich99@mail.ru", LotId = 1 };
            await lotCommentOperationsHandler.AddCommentAsync(lotComment);
            var lotComment2 = new LotComment { Message = "Comment2", UserId = "veklich99@mail.ru", LotId = 1 };
            await lotCommentOperationsHandler.AddCommentAsync(lotComment2);

            var resultLot = await lotOperationsHandler.GetLotAsync(1, l => l.LotComments, l => l.SellerUser);
            resultLot.LotComments = lotCommentOperationsHandler.GetLotComments(resultLot.Id).ToList();
            var resultUser = await userAccountOperationsHandler.GetUserAccountAsync("veklich99@mail.ru");
            resultUser.LotComments = lotCommentOperationsHandler.GetUserComments(resultUser.Email).ToList();

            Assert.AreEqual(1, (await lotOperationsHandler.GetAllLotsAsync()).Count());
            Assert.AreEqual(2, resultLot.LotComments.Count());
            Assert.AreEqual(2, resultUser.LotComments.Count());
            Assert.AreEqual("Comment1", resultLot.LotComments[0].Message);
            Assert.AreEqual("DefaultUser", resultLot.LotComments[0].User.Name);
            Assert.AreEqual("Comment2", resultLot.LotComments[1].Message);
            Assert.AreEqual("DefaultUser", resultLot.LotComments[1].User.Name);
        }

        [Test]
        public async Task AddCommentAsync_InvalidInput_ThrowsArgumentException()
        {
            var lot = new Lot { SellerUserId = "veklich99@mail.ru", SellDate = DateTime.Now, Name = "Name1" };
            await lotOperationsHandler.AddLotAsync(lot, "", "");
            await userAccountOperationsHandler.AddUserAccountAsync(new UserAccountInfo { Name = "Commenter", Email = "veklich99@gmail.com" });
            var invalidUserIdComment = new LotComment { Message = "Comment1", UserId = "veklich98@gmail.com", LotId = 1 };
            var invalidLotIdComment = new LotComment { Message = "Comment1", UserId = "veklich99@gmail.com", LotId = 2 };

            Assert.ThrowsAsync<ArgumentException>(() => lotCommentOperationsHandler.AddCommentAsync(invalidUserIdComment));
            Assert.ThrowsAsync<ArgumentException>(() => lotCommentOperationsHandler.AddCommentAsync(invalidLotIdComment));
        }
    }
}
