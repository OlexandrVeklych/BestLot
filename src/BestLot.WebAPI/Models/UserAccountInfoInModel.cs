using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.WebAPI.Models
{
    public class UserAccountInfoInModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [DataType(DataType.Text)]
        public string Surname { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string TelephoneNumber { get; set; }

        public List<LotInModel> Lots { get; set; }
        public List<LotCommentInModel> LotComments { get; set; }
    }
}
