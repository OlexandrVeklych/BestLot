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
        //hostingEnvironmentPath - physical path to WebAPI folder
        //requestUriLeftPart - URL
        private LotSalesHandler(double refreshTimeMillisecs, double checkTimeMillisecs, string hostingEnvironment, string requestUriLeftPart)
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LotEntity, LotArchiveEntity>();
            }).CreateMapper();
            LotId_SellDatePairs = new Dictionary<int, DateTime>();
            refreshTimer = new Timer(refreshTimeMillisecs);
            refreshTimer.AutoReset = true;
            refreshTimer.Elapsed += RefreshLots;
            checkTimer = new Timer(checkTimeMillisecs);
            checkTimer.AutoReset = true;
            checkTimer.Elapsed += CheckLots;
            this.hostingEnvironment = hostingEnvironment;
            this.requestUriLeftPart = requestUriLeftPart;
        }

        public LotSalesHandler(IUnitOfWork unitOfWork,
            ILotOperationsHandler lotOperationsHandler,
            IUserAccountOperationsHandler userAccountOperationsHandler,
            double refreshTimeMillisecs,
            double checkTimeMillisecs,
            string hostingEnvironment,
            string requestUriLeftPart)
            : this(refreshTimeMillisecs, checkTimeMillisecs, hostingEnvironment, requestUriLeftPart)
        {
            this.UoW = unitOfWork;
            this.lotOperationsHandler = lotOperationsHandler;
            this.userAccountOperationsHandler = userAccountOperationsHandler;
        }


        private ILotOperationsHandler lotOperationsHandler;
        private readonly IUserAccountOperationsHandler userAccountOperationsHandler;
        private Timer refreshTimer;
        private Timer checkTimer;
        private IUnitOfWork UoW;
        private IMapper mapper;
        private readonly string hostingEnvironment;
        private readonly string requestUriLeftPart;
        public Dictionary<int, DateTime> LotId_SellDatePairs { get; private set; }

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

        //Checks sell dates in dictionary in memory
        private void CheckLots(object sender, ElapsedEventArgs e)
        {
            if (!LotId_SellDatePairs.Any())
                return;
            List<int> keys = new List<int>(LotId_SellDatePairs.Keys);
            foreach (int key in keys)
            {
                if (LotId_SellDatePairs[key].CompareTo(DateTime.Now) <= 0)
                {
                    SellLot(key);
                    LotId_SellDatePairs.Remove(key);
                }
            }
        }

        //Loads lots sell dates into dictionary
        private void RefreshLots(object sender, ElapsedEventArgs e)
        {
            LotId_SellDatePairs.Clear();
            foreach (Lot lot in lotOperationsHandler.GetAllLots())
            {
                LotId_SellDatePairs.Add(lot.Id, lot.SellDate);
            }
        }

        private void SellLot(int lotId)
        {
            //Lot lotForSale = lotOperationsHandler.GetLot(lotId);
            //lotForSale.SellerUser = userAccountOperationsHandler.GetSellerUser(lotId);
            //UserAccountInfo buyerUser = mapper.Map<UserAccountInfo>(UoW.UserAccounts.Get(lotForSale.BuyerUserId));
            //lotForSale.Sell(buyerUser);

            UoW.LotArchive.Add(mapper.Map<LotArchiveEntity>(UoW.Lots.Get(lotId)));
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
