﻿using BestLot.BusinessLogicLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.BidLogic
{
    internal interface IBidPlacer
    {
        void PlaceBid(Lot lot, string buyerUserId, double price);
    }
}
