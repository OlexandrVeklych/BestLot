using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.WebAPI.Models
{
    public class LotCommentModel
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [MaxLength(250)]
        public string Message { get; set; }
        public string UserId { get; set; }
        public UserAccountInfoModel User { get; set; }
        [Range(1, 10)]
        public int Rating { get; set; }
        public int LotId { get; set; }
        public LotModel Lot { get; set; }
    }
}
