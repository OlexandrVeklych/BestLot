using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.WebAPI.Models
{
    public class LotPhotoInModel
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public int LotId { get; set; }
        public LotInModel Lot { get; set; }
    }
}
