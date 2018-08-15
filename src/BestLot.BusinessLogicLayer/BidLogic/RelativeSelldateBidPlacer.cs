using BestLot.BusinessLogicLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.BidLogic
{
    //For lots that need to be sold if no bids were placed for certain time
    public class RelativeSellDateBidPlacer : IBidPlacer
    {
        public void PlaceBid(Lot lot, string buyerUserId, double price)
        {
            if (price < lot.Price + lot.MinStep)
                throw new ArgumentException("Min. availible bid is " + (lot.Price + lot.MinStep));
            lot.Price = price;
            lot.BuyerUserId = buyerUserId;
            DateTime now = DateTime.Now;
            //Sell date is calculated, so period from last bid to selling stays the same
            lot.SellDate = now.Add(lot.SellDate.Subtract(lot.StartDate));
            //Here start date is actually date of last bid
            lot.StartDate = now;
        }
    }
}
