using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.DataAccessLayer.Entities
{
    public class UserAccountInfoEntity
    {
        //Key
        public string Email { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string TelephoneNumber { get; set; }
        public ICollection<LotEntity> Lots { get; set; }
        public ICollection<LotCommentEntity> LotComments { get; set; }
    }
}
