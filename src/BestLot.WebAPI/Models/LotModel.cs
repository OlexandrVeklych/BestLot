using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.WebAPI.Models
{
    public class LotModel
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [DataType(DataType.Text)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Category { get; set; }

        public string SellerUserId { get; set; }
        public UserAccountInfoModel SellerUser { get; set; }
        public string BuyerUserId { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        public double MinStep { get; set; }

        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SellDate { get; set; }

        public List<LotPhotoModel> LotPhotos { get; set; }
        public List<LotCommentModel> LotComments { get; set; }

        [Required]
        public int BidPlacer { get; set; }
    }
}
