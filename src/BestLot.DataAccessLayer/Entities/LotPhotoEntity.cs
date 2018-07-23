using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.DataAccessLayer.Entities
{
    public class LotPhotoEntity
    {
        public int Id { get; set; }
        public byte[] Photo { get; set; }
        public string Description { get; set; }
        public int LotId { get; set; }
        public LotEntity Lot { get; set; }
    }
}
