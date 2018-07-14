using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.UnitOfWork;

namespace UnitTestProject
{
    public class UnitTestDependencyResolver
    {
        public static IUnitOfWork Resolve()
        {
            return new UnitOfWork("UnitTestLotContext", "UnitTestLotArchiveContext"); ;
        }
    }
}
