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
        [MaxLength(30)]
        public string Name { get; set; }

        [DataType(DataType.Text)]
        [MaxLength(150)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [MaxLength(30)]
        public string Category { get; set; }

        public string SellerUserId { get; set; }
        public UserAccountInfoModel SellerUser { get; set; }
        public string BuyerUserId { get; set; }

        [Required]
        [Range(0, Int32.MaxValue)]
        public double Price { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public string Currency { get; set; }

        [Required]
        [Range(0, Int32.MaxValue)]
        public double MinStep { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SellDate { get; set; }

        public List<LotPhotoModel> LotPhotos { get; set; }
        public List<LotCommentModel> LotComments { get; set; }

        [Required]
        [BidPlacer("Relative", "Determined", "Wrong bidplacer")]
        public string BidPlacer { get; set; }
    }
}
