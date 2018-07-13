using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.UnitOfWork;

namespace UnitTestProject
{
    public static class UnitTestDependencyResolver
    {
        private static IUnitOfWork unitOfWork;

        static UnitTestDependencyResolver()
        {
            unitOfWork = new UnitOfWork("UnitTestLotContext", "UnitTestLotArchiveContext");
        }

        public static IUnitOfWork Resolve()
        {
            return unitOfWork;
        }
    }
}
