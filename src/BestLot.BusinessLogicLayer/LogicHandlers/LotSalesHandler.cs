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
        public LotSalesHandler(IUnitOfWork unitOfWork,
            ILotOperationsHandler lotOperationsHandler,
            IUserAccountOperationsHandler userAccountOperationsHandler,
            double refreshTimeMillisecs,
            double checkTimeMillisecs,
            string hostingEnvironment,
            string requestUriLeftPart,
            bool sendEmail = true)
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
            this.UoW = unitOfWork;
            this.lotOperationsHandler = lotOperationsHandler;
            this.userAccountOperationsHandler = userAccountOperationsHandler;
            this.sendEmail = sendEmail;
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
        private bool sendEmail;

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
            lock (LotId_SellDatePairs)
            {
                List<int> keys = new List<int>(LotId_SellDatePairs.Keys);
                if (!keys.Any())
                    return;
                foreach (int key in keys)
                {
                    if (LotId_SellDatePairs[key].CompareTo(DateTime.Now) <= 0)
                    {
                        SellLotAsync(key).Wait();
                        LotId_SellDatePairs.Remove(key);
                    }
                }
            }
        }

        //Loads lots sell dates into dictionary
        private void RefreshLots(object sender, ElapsedEventArgs e)
        {
            lock (LotId_SellDatePairs)
            {
                LotId_SellDatePairs.Clear();
                foreach (Lot lot in lotOperationsHandler.GetAllLots())
                {
                    LotId_SellDatePairs.Add(lot.Id, lot.SellDate);
                }
            }
        }

        private async Task SellLotAsync(int lotId)
        {
            if (sendEmail)
            {
                Lot lotForSale = await lotOperationsHandler.GetLotAsync(lotId);
                lotForSale.SellerUser = await userAccountOperationsHandler.GetSellerUserAsync(lotId);
                UserAccountInfo buyerUser = await userAccountOperationsHandler.GetBuyerUserAsync(lotId);
                lotForSale.Sell(buyerUser);
            }

            UoW.LotArchive.Add(mapper.Map<LotArchiveEntity>(await UoW.Lots.GetAsync(lotId)));
            await UoW.SaveArchiveChangesAsync();
            await lotOperationsHandler.DeleteLotAsync(lotId, hostingEnvironment, requestUriLeftPart);
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                StopSalesHandler();
                UoW.Dispose();
                if (disposing)
                {
                    mapper = null;
                    UoW = null;
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
