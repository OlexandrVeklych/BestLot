using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Timers;
using DataAccessLayer.UnitOfWork;
using DataAccessLayer.Entities;
using BusinessLogicLayer.Models;
using AutoMapper;

namespace BusinessLogicLayer.LogicHandlers
{
    public class LotSalesHandler
    {
        public LotSalesHandler(IUnitOfWork unitOfWork, double refreshTimeSecs)
        {
            UoW = unitOfWork;
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LotEntity, Lot>();
                cfg.CreateMap<UserAccountInfoEntity, UserAccountInfo>();
            }).CreateMapper();
            timer = new Timer(refreshTimeSecs);
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(CheckLots);          
        }

        private Timer timer;
        private IUnitOfWork UoW;
        private IMapper mapper;
        private Dictionary<int, DateTime> lotsSellDate;


        public void Run()
        {
            RefreshLots();

            timer.Start();
        }

        public void CheckLots(object sender, ElapsedEventArgs e)
        {
            foreach (var idDatePair in lotsSellDate)
            {
                if (idDatePair.Value.CompareTo(DateTime.Now) <= 0)
                    SellLot(idDatePair.Key);
            }
            Run();
        }

        public void RefreshLots()
        {
            lotsSellDate = new Dictionary<int, DateTime>();
            foreach (Lot lot in mapper.Map<IEnumerable<LotEntity>, IEnumerable<Lot>>(UoW.Lots.GetAll()))
            {
                lotsSellDate.Add(lot.Id, lot.SellDate);
            }
        }

        private void SellLot(int lotId)
        {
            Lot lotForSale = mapper.Map<Lot>(UoW.Lots.Get(lotId, l => l.SellerUser));
            UserAccountInfo buyerUser = mapper.Map<UserAccountInfo>(UoW.UserAccounts.Get(lotForSale.BuyerUserId));

            lotForSale.Sell(buyerUser);

            UoW.LotArchive.Add(UoW.Lots.Get(lotId));
            UoW.Lots.Delete(lotId);
            UoW.SaveArchiveChanges();
            UoW.SaveChanges();
        }
    }
}
