using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.Models
{
    public class UserAccountInfo
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string TelephoneNumber { get; set; }
        public List<Lot> Lots { get; set; }
        public List<LotComment> LotComments { get; set; }
    }
}
