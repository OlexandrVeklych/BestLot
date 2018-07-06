using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Models
{
    public class CommentModel
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public dynamic User { get; set; } // Change this to User from WebAPI
        public int Rating { get; set; }
    }
}
