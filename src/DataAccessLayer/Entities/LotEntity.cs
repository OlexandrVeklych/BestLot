﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Entities
{
    public class LotEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SellerUserId { get; set; }
        public UserAccountInfoEntity SellerUser { get; set; }
        public int BuyerUserId { get; set; }
        public double Price { get; set; }
        public double MinStep { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime SellDate { get; set; }
        public List<LotPhotoEntity> LotPhotos { get; set; }
        public List<LotCommentEntity> Comments { get; set; }
    }
}