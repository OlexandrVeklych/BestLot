using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Models
{
    public class LotComment
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public UserAccountInfo User { get; set; }
        public int Rating { get; set; }
    }
}
