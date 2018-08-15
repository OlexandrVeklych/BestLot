using BestLot.BusinessLogicLayer.Exceptions;
using BestLot.BusinessLogicLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.BidLogic
{
    //For lots that need to be sold at certain time, not depending on bids
    class DeterminedSellDateBidPlacer : IBidPlacer
    {
        public void PlaceBid(Lot lot, string buyerUserId, double price)
        {
            if (price < lot.Price + lot.MinStep)
                throw new WrongModelException("Min. availible bid is " + (lot.Price + lot.MinStep));
            lot.Price = price;
            lot.BuyerUserId = buyerUserId;
        }
    }
}
