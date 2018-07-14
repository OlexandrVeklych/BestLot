using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Entities
{
    public class LotCommentEntity
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public UserAccountInfoEntity User { get; set; }
        public int Rating { get; set; }
        public int LotId { get; set; }
        public LotEntity Lot { get; set; }
    }
}
