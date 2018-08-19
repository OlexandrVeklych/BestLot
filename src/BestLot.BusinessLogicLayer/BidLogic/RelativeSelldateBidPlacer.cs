using BestLot.BusinessLogicLayer.Exceptions;
using BestLot.BusinessLogicLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.BidLogic
{
    //For lots that need to be sold when no bids were placed for certain time
    //That time is difference between start and sell dates
    //e.g.: users have 1 minute to place bids, then:
    //StartDate = 01.01.2018 11:00
    //SellDate = 01.01.2018 11:01
    internal class RelativeSellDateBidPlacer : IBidPlacer
    {
        public void PlaceBid(Lot lot, string buyerUserId, double price)
        {
            if (price < lot.Price + lot.MinStep)
                throw new WrongModelException("Min. availible bid is " + (lot.Price + lot.MinStep));
            if (lot.SellDate.CompareTo(DateTime.Now) < 0)
                throw new WrongModelException("Lot is already sold");
            lot.Price = price;
            lot.BuyerUserId = buyerUserId;
            DateTime now = DateTime.Now;
            //Sell date is calculated, so period from start date to sell date stays the same
            //New sell date = previous sell date + difference between previous sell date and previous start date
            lot.SellDate = now.Add(lot.SellDate.Subtract(lot.StartDate));
            //Here start date is actually the date of last bid
            lot.StartDate = now;
        }
    }
}
