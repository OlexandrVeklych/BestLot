using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BestLot.WebAPI.Models
{
    public class BidPlacerAttribute : ValidationAttribute
    {
        private readonly string[] _possibleValues;

        public BidPlacerAttribute(params string[] possibleValues)
        {
            _possibleValues = possibleValues;
        }

        public override bool IsValid(object value)
        {
            return _possibleValues.Contains((string)value);
        }
    }
}