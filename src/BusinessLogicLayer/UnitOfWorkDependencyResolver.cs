using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.UnitOfWork;

namespace BusinessLogicLayer
{
    public static class UnitOfWorkDependencyResolver
    {
        private static IUnitOfWork unitOfWork;

        static UnitOfWorkDependencyResolver()
        {
            unitOfWork = new UnitOfWork("LotContext", "LotArchiveContext");
        }

        public static IUnitOfWork Resolve()
        {
            return unitOfWork;
        }
    }
}
