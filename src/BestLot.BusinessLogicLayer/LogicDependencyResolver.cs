﻿using System;
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
            return new LotSalesHandler(ResolveUnitOfWork(), ResolveLotOperationsHandler(), ResolveUserAccountOperationsHandler(), refreshTimeMillisecs, checkTimeMillisecs, @"C:\Users\Olexa\source\repos\BestLot\src\BestLot.WebAPI", "http://localhost:63959");
        }

        public static ILotOperationsHandler ResolveLotOperationsHandler()
        {
            var correctLotPhotoOperationsHandler = ResolveLotPhotoOperationsHandler(new LotOperationsHandler(ResolveUnitOfWork(), new LotPhotoOperationsHandler(null, null)));
            correctLotPhotoOperationsHandler.LotOperationsHandler.LotPhotoOperationsHandler = correctLotPhotoOperationsHandler;
            return new LotOperationsHandler(ResolveUnitOfWork(), correctLotPhotoOperationsHandler);
        }

        public static IUserAccountOperationsHandler ResolveUserAccountOperationsHandler()
        {
            return new UserAccountOperationsHandler(ResolveUnitOfWork(), ResolveLotOperationsHandler(), ResolveLotPhotoOperationsHandler());
        }

        public static ILotCommentOperationsHandler ResolveLotCommentOperationsHandler()
        {
            return new LotCommentOperationsHandler(ResolveUnitOfWork(), ResolveLotOperationsHandler(), ResolveUserAccountOperationsHandler());
        }

        public static ILotPhotoOperationsHandler ResolveLotPhotoOperationsHandler()
        {
            return new LotPhotoOperationsHandler(ResolveUnitOfWork(), ResolveLotOperationsHandler());
        }

        public static ILotPhotoOperationsHandler ResolveLotPhotoOperationsHandler(ILotOperationsHandler lotOperationsHandler)
        {
            return new LotPhotoOperationsHandler(ResolveUnitOfWork(), lotOperationsHandler);
        }

        public static ILotArchiveOperationsHandler ResolveLotArchiveOperationsHandler()
        {
            return new LotArchiveOperationsHandler(ResolveUnitOfWork());
        }
    }
}
