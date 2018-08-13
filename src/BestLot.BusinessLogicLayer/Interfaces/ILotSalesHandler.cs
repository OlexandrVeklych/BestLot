using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.Interfaces
{
    public interface ILotSalesHandler
    {
        void RunSalesHandler();
        void StopSalesHandler();
        Dictionary<int, DateTime> LotId_SellDatePairs { get; }
    }
}
