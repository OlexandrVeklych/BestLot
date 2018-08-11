using BestLot.BusinessLogicLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.BidLogic
{
    public interface IBidPlacer
    {
        void PlaceBid(IBidable bidable, string buyerUserId, double price);
    }
}
