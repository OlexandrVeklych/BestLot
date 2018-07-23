using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.WebAPI.Models
{
    public class LotModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SellerUserId { get; set; }
        public UserAccountInfoModel SellerUser { get; set; }
        public int BuyerUserId { get; set; }
        public double Price { get; set; }
        public double MinStep { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime SellDate { get; set; }
        public List<LotPhotoModel> LotPhotos { get; set; }
        public List<LotCommentModel> LotComments { get; set; }
    }
}
