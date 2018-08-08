using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer
{
    public class RelativeSelldateBidPlacer : IBidPlacer
    {
        public void PlaceBid(IBidable bidable, string buyerUserId, double price)
        {
            if (price < bidable.Price + bidable.MinStep)
                throw new ArgumentException("Min. availible bid is " + (bidable.Price + bidable.MinStep));
            bidable.Price = price;
            bidable.BuyerUserId = buyerUserId;
            DateTime now = DateTime.Now;
            bidable.SellDate = now.Add(bidable.SellDate.Subtract(bidable.StartDate));
            bidable.StartDate = now;
        }
    }
}
