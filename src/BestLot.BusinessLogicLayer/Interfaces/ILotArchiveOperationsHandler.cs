using BestLot.BusinessLogicLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.Interfaces
{
    public interface ILotArchiveOperationsHandler : IDisposable
    {
        IQueryable<Lot> GetUserArchivedLots(string userEmail);
        Task<IQueryable<Lot>> GetUserArchivedLotsAsync(string userEmail);
    }
}
