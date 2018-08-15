using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.Exceptions
{
    public class WrongIdException : Exception
    {
        public WrongIdException(string modelName) : base("Wrong " + modelName + " Id") { }
    }
}
