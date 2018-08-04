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

        public static ILotOperationsHandler ResolveLotOperationsHandler()
        {
            return new LotOperationsHandler(ResolveUnitOfWork());
        }

        public static IUserAccountOperationsHandler ResolveUserAccountOperationsHandler()
        {
            return new UserAccountOperationsHandler(ResolveUnitOfWork());
        }

        public static ILotCommentOperationsHandler ResolveLotCommentsOperationsHandler()
        {
            return new LotCommentOperationsHandler(ResolveUnitOfWork());
        }

        public static ILotPhotoOperationsHandler ResolveLotPhotosOperationsHandler()
        {
            return new LotPhotoOperationsHandler(ResolveUnitOfWork());
        }
    }
}
