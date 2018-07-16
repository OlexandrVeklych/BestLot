﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Models
{
    public class Lot
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SellerUserId { get; set; }
        public UserAccountInfo SellerUser { get; set; }
        public int BuyerUserId { get; set; }
        public double Price { get; set; }
        public double MinStep { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime SellDate { get; set; }
        public List<LotPhoto> LotPhotos { get; set; }
        public List<LotComment> Comments { get; set; }
    }
}