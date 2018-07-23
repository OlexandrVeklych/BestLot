using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestLot.DataAccessLayer.UnitOfWork;
using BestLot.BusinessLogicLayer.LogicHandlers;

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

        public static ILotOperationsHandler ResloveLotOperationsHandler(IUnitOfWork unitOfWork)
        {
            return new LotOperationsHandler(unitOfWork);
        }

        public static IUserAccountOperationsHandler ResloveUserAccountOperationsHandler(IUnitOfWork unitOfWork)
        {
            return new UserAccountOperationsHandler(unitOfWork);
        }
    }
}
