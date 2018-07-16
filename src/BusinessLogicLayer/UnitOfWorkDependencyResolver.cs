using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.UnitOfWork;

namespace BusinessLogicLayer
{
    public class UnitOfWorkDependencyResolver
    {
        public static IUnitOfWork Resolve()
        {
            return new UnitOfWork("LotContext", "LotArchiveContext");
        }
    }
}
