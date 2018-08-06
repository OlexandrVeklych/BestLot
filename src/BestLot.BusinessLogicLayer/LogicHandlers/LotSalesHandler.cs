using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Timers;
using BestLot.DataAccessLayer.UnitOfWork;
using BestLot.DataAccessLayer.Entities;
using BestLot.BusinessLogicLayer.Models;
using AutoMapper;
using BestLot.BusinessLogicLayer.Interfaces;

namespace BestLot.BusinessLogicLayer.LogicHandlers
{
    public class LotSalesHandler : ILotSalesHandler, IDisposable
    {
        private LotSalesHandler(double refreshTimeMillisecs, double checkTimeMillisecs)
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LotEntity, Lot>();
                cfg.CreateMap<LotEntity, ArchiveLotEntity>();
                cfg.CreateMap<UserAccountInfoEntity, UserAccountInfo>();
            }).CreateMapper();
            lotsSellDate = new Dictionary<int, DateTime>();
            refreshTimer = new Timer(refreshTimeMillisecs);
            refreshTimer.AutoReset = true;
            refreshTimer.Elapsed += RefreshLots;
            checkTimer = new Timer(checkTimeMillisecs);
            checkTimer.AutoReset = true;
            checkTimer.Elapsed += CheckLots;
        }

        public LotSalesHandler(IUnitOfWork unitOfWork, double refreshTimeMillisecs, double checkTimeMillisecs) :this(refreshTimeMillisecs, checkTimeMillisecs)
        {
            this.UoW = unitOfWork;
        }

        private Timer refreshTimer;
        private Timer checkTimer;
        private IUnitOfWork UoW;
        private IMapper mapper;
        public Dictionary<int, DateTime> lotsSellDate { get; private set; }

        public void StopSalesHandler()
        {
            refreshTimer.Stop();
            refreshTimer.EndInit();
            checkTimer.Stop();
            checkTimer.EndInit();
        }

        public void RunSalesHandler()
        {
            refreshTimer.Start();
            checkTimer.Start();
        }

        private void CheckLots(object sender, ElapsedEventArgs e)
        {
            foreach (var idDatePair in lotsSellDate)
            {
                if (idDatePair.Value.CompareTo(DateTime.Now) <= 0)
                    SellLot(idDatePair.Key);
            }
        }

        private void RefreshLots(object sender, ElapsedEventArgs e)
        {
            lotsSellDate.Clear();
            foreach (Lot lot in mapper.Map<IEnumerable<Lot>>(UoW.Lots.GetAll()))
            {
                lotsSellDate.Add(lot.Id, lot.SellDate);
            }
        }

        private void SellLot(int lotId)
        {
            //Lot lotForSale = mapper.Map<Lot>(UoW.Lots.Get(lotId, l => l.SellerUser));
            //UserAccountInfo buyerUser = mapper.Map<UserAccountInfo>(UoW.UserAccounts.Get(lotForSale.BuyerUserId));
            //
            //lotForSale.Sell(buyerUser);

            lotsSellDate.Remove(lotId);

            UoW.LotArchive.Add(mapper.Map<ArchiveLotEntity>(UoW.Lots.Get(lotId)));
            UoW.Lots.Delete(lotId);
            UoW.SaveArchiveChanges();
            UoW.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    StopSalesHandler();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
