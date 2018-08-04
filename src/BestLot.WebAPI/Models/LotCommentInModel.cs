using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.WebAPI.Models
{
    public class LotCommentInModel
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }
        public string UserId { get; set; }
        public UserAccountInfoInModel User { get; set; }
        public int Rating { get; set; }
        public int LotId { get; set; }
        public LotInModel Lot { get; set; }
    }
}
