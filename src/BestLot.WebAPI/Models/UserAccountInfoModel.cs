using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.WebAPI.Models
{
    public class UserAccountInfoModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [MaxLength(20)]
        public string Name { get; set; }

        [DataType(DataType.Text)]
        [MaxLength(20)]
        public string Surname { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string TelephoneNumber { get; set; }

        public List<LotModel> Lots { get; set; }
        public List<LotCommentModel> LotComments { get; set; }
    }
}
