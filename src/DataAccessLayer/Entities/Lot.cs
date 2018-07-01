using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Entities
{
    public class Lot
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public dynamic User { get; set; } // Change this to User from WebAPI
        public double StartPrice { get; set; }
        public List<LotPhoto> LotPhotos { get; set; }
        public List<Comment> Cooments { get; set; }
    }
}
