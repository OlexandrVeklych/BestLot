﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.WebAPI.Models
{
    public class LotPhotoModel
    {
        public int Id { get; set; }
        public string Path { get; set; }
        [MaxLength(50)]
        public string Description { get; set; }
        public int LotId { get; set; }
        public LotModel Lot { get; set; }
    }
}
