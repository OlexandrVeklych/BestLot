using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.Models
{
    public class LotPhoto
    {
        public int Id { get; set; }
        public byte[] Photo { get; set; }
        public string Description { get; set; }
        public string LotId { get; set; }
        public Lot Lot { get; set; }
    }
}
