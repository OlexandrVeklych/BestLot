using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogicLayer.LogicHandlers;
using DataAccessLayer.UnitOfWork;

namespace BusinessLogicLayer
{
    public class LogicDependencyResolver
    {
        public static IUnitOfWork ResolveUnitOfWork()
        {
            return new UnitOfWork("LotContext", "LotArchiveContext");
        }

        public static ILotSalesHandler ResolveLotSalesHandler(double refreshTimeMillisecs, double checkTimeMillisecs)
        {
            return new LotSalesHandler(ResolveUnitOfWork(), refreshTimeMillisecs, checkTimeMillisecs);
        }

        public static ILotOperationsHandler ResloveLotOperationsHandler()
        {
            return new LotOperationsHandler(ResolveUnitOfWork());
        }

        public static IUserAccountOperationsHandler ResloveUserAccountOperationsHandler()
        {
            return new UserAccountOperationsHandler(ResolveUnitOfWork());
        }
    }
}
