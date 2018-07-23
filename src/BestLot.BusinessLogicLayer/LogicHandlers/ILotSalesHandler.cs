using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.LogicHandlers
{
    public interface ILotSalesHandler
    {
        void RunSalesHandler();
        void StopSalesHandler();
        Dictionary<int, DateTime> lotsSellDate { get; }
    }
}
