using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.WebAPI.Models
{
    public class LotCommentOutModel
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public UserAccountInfoOutModel User { get; set; }
        public int Rating { get; set; }
        public int LotId { get; set; }
    }
}
