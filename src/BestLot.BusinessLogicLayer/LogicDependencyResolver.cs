using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestLot.BusinessLogicLayer.Interfaces;
using BestLot.BusinessLogicLayer.LogicHandlers;
using BestLot.DataAccessLayer.UnitOfWork;

namespace BestLot.BusinessLogicLayer
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

        public static ILotCommentsOperationsHandler ResloveLotCommentsOperationsHandler()
        {
            return new LotCommentsOperationsHandler(ResolveUnitOfWork());
        }

        public static ILotPhotosOperationsHandler ResloveLotPhotosOperationsHandler()
        {
            return new LotPhotoOperationsHandler(ResolveUnitOfWork());
        }
    }
}
