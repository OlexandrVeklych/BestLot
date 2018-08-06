using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestLot.DataAccessLayer.UnitOfWork;
using BestLot.BusinessLogicLayer.LogicHandlers;
using BestLot.BusinessLogicLayer.Interfaces;

namespace BestLot.UnitTests
{
    public class UnitTestDependencyResolver
    {
        public static IUnitOfWork ResolveUnitOfWork()
        {
            return new UnitOfWork("UnitTestLotContext", "UnitTestLotArchiveContext"); ;
        }

        public static ILotSalesHandler ResolveLotSalesHandler(IUnitOfWork unitOfWork, double refreshTimeMillisecs, double checkTimeMillisecs)
        {
            return new LotSalesHandler(unitOfWork, refreshTimeMillisecs, checkTimeMillisecs);
        }

        public static ILotOperationsHandler ResolveLotOperationsHandler(IUnitOfWork unitOfWork)
        {
            var correctLotPhotoOperationsHandler = ResolveLotPhotoOperationsHandler(unitOfWork, new LotOperationsHandler(unitOfWork, new LotPhotoOperationsHandler(null, null)));
            correctLotPhotoOperationsHandler.LotOperationsHandler.LotPhotoOperationsHandler = correctLotPhotoOperationsHandler;
            return new LotOperationsHandler(unitOfWork, correctLotPhotoOperationsHandler);
        }

        public static IUserAccountOperationsHandler ResolveUserAccountOperationsHandler(IUnitOfWork unitOfWork)
        {
            return new UserAccountOperationsHandler(unitOfWork, ResolveLotOperationsHandler(unitOfWork), ResolveLotPhotoOperationsHandler(unitOfWork));
        }

        public static ILotCommentOperationsHandler ResolveLotCommentOperationsHandler(IUnitOfWork unitOfWork)
        {
            return new LotCommentOperationsHandler(unitOfWork, ResolveLotOperationsHandler(unitOfWork), ResolveUserAccountOperationsHandler(unitOfWork));
        }

        public static ILotPhotoOperationsHandler ResolveLotPhotoOperationsHandler(IUnitOfWork unitOfWork)
        {
            return new LotPhotoOperationsHandler(unitOfWork, ResolveLotOperationsHandler(unitOfWork));
        }

        public static ILotPhotoOperationsHandler ResolveLotPhotoOperationsHandler(IUnitOfWork unitOfWork, ILotOperationsHandler lotOperationsHandler)
        {
            return new LotPhotoOperationsHandler(unitOfWork, lotOperationsHandler);
        }
    }
}
