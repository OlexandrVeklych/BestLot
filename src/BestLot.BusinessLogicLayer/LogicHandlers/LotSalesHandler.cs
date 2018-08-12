using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private LotSalesHandler(double refreshTimeMillisecs, double checkTimeMillisecs, string hostingEnvironment, string requestUriLeftPart)
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
            this.hostingEnvironment = hostingEnvironment;
            this.requestUriLeftPart = requestUriLeftPart;
        }

        public LotSalesHandler(IUnitOfWork unitOfWork, ILotOperationsHandler lotOperationsHandler, IUserAccountOperationsHandler userAccountOperationsHandler, double refreshTimeMillisecs, double checkTimeMillisecs, string hostingEnvironment, string requestUriLeftPart) : this(refreshTimeMillisecs, checkTimeMillisecs, hostingEnvironment, requestUriLeftPart)
        {
            this.UoW = unitOfWork;
            this.lotOperationsHandler = lotOperationsHandler;
            this.userAccountOperationsHandler = userAccountOperationsHandler;
        }


        private ILotOperationsHandler lotOperationsHandler;
        private IUserAccountOperationsHandler userAccountOperationsHandler;
        private Timer refreshTimer;
        private Timer checkTimer;
        private IUnitOfWork UoW;
        private IMapper mapper;
        private string hostingEnvironment;
        private string requestUriLeftPart;
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
            List<int> keys = new List<int>(lotsSellDate.Keys);
            foreach(int key in keys)
            {
                if (lotsSellDate[key].CompareTo(DateTime.Now) <= 0)
                {
                    SellLot(key);
                    lotsSellDate.Remove(key);
                }
            }
        }

        private void RefreshLots(object sender, ElapsedEventArgs e)
        {
            lotsSellDate.Clear();
            foreach (Lot lot in lotOperationsHandler.GetAllLots())
            {
                lotsSellDate.Add(lot.Id, lot.SellDate);
            }
        }

        private void SellLot(int lotId)
        {
            Lot lotForSale = mapper.Map<Lot>(lotOperationsHandler.GetLot(lotId));
            lotForSale.SellerUser = userAccountOperationsHandler.GetSellerUser(lotId);
            UserAccountInfo buyerUser = mapper.Map<UserAccountInfo>(UoW.UserAccounts.Get(lotForSale.BuyerUserId));
            lotForSale.Sell(buyerUser);

            UoW.LotArchive.Add(mapper.Map<ArchiveLotEntity>(UoW.Lots.Get(lotId)));
            lotOperationsHandler.DeleteLot(lotId, hostingEnvironment, requestUriLeftPart);
            UoW.SaveArchiveChanges();
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
