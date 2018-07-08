using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.UnitOfWork;
using DataAccessLayer.Entities;
using BusinessLogicLayer.Models;
using AutoMapper;

namespace BusinessLogicLayer
{
    public class LotSalesController
    {
        public LotSalesController(IUnitOfWork unitOfWork)
        {
            UoW = unitOfWork;
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Lot, LotModel>();
            }).CreateMapper();
            lotsSellDate = new Dictionary<int, DateTime>();
            foreach (LotModel lot in mapper.Map<IEnumerable<Lot>, IEnumerable<LotModel>>(UoW.Lots.GetAll()))
            {
                lotsSellDate.Add(lot.Id, lot.SellDate);
            }
        }

        private IUnitOfWork UoW;
        private IMapper mapper;
        private Dictionary<int, DateTime> lotsSellDate;

        public void CheckLots()
        {
            foreach (var idDatePair in lotsSellDate)
            {
                if (idDatePair.Value.CompareTo(DateTime.Now) <= 0)
                {
                    SellLot(idDatePair.Key);
                    UoW.LotArchive.Add(UoW.Lots.Get(idDatePair.Key));
                    UoW.Lots.Delete(idDatePair.Key);
                    UoW.SaveArchiveChanges();
                    UoW.SaveChanges();
                }
            }
        }

        public void RefreshLots()
        {
            lotsSellDate = new Dictionary<int, DateTime>();
            foreach (LotModel lot in mapper.Map<IEnumerable<Lot>, IEnumerable<LotModel>>(UoW.Lots.GetAll()))
            {
                lotsSellDate.Add(lot.Id, lot.SellDate);
            }
        }
        
        private void SellLot(int lotId)
        {
            throw new NotImplementedException(); //Implement sending Email
        }
    }
}
