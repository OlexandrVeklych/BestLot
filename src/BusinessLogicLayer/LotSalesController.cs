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
        }

        private IUnitOfWork UoW;
        private IMapper mapper;

        public void CheckLots()
        {
            IEnumerable<LotModel> lots = mapper.Map<IEnumerable<Lot>, IEnumerable<LotModel>>(UoW.Lots.GetAll());
            foreach (LotModel lot in lots)
            {
                if (lot.EndDate.CompareTo(DateTime.Now) <= 0)
                {
                    UoW.Lots.Get(lot.Id).IsSold = true;
                    UoW.SaveChanges();
                    SellLot(lot);
                }
            }
        }
        
        private void SellLot(LotModel lot)
        {
            throw new NotImplementedException();
        }
    }
}
