using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.Exceptions
{
    public class WrongModelException : Exception
    {
        public WrongModelException(string message) : base (message){ }
    }
}
