using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer
{
    public interface IBidable
    {
        double Price { get; set; }
        double MinStep { get; set; }
        string BuyerUserId { get; set; }
        DateTime StartDate { get; set; }
        DateTime SellDate { get; set; }
    }
}
