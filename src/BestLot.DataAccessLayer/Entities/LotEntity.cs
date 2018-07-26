using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.DataAccessLayer.Entities
{
    public class LotEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string SellerUserId { get; set; }
        public UserAccountInfoEntity SellerUser { get; set; }
        public string BuyerUserId { get; set; }
        public double Price { get; set; }
        public double MinStep { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime SellDate { get; set; }
        public ICollection<LotPhotoEntity> LotPhotos { get; set; }
        public ICollection<LotCommentEntity> LotComments { get; set; }
    }
}
