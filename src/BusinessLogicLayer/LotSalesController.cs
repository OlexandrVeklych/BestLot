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

namespace BusinessLogicLayer
{
    public class LotSalesController
    {
        public LotSalesController(IUnitOfWork unitOfWork, double refreshTimeSecs)
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
            Lot lotForSale = mapper.Map<Lot>(UoW.Lots.Get(lotId));
            if (lotForSale.BuyerUserId != 0)
            {
                UserAccountInfo sellerUser = mapper.Map<UserAccountInfo>(UoW.UserAccounts.Get(lotForSale.SellerUserId));
                UserAccountInfo buyerUser = mapper.Map<UserAccountInfo>(UoW.UserAccounts.Get(lotForSale.BuyerUserId));

                SmtpClient client = new SmtpClient
                {
                    Port = 25,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Host = "smtp.gmail.com"
                };


                MailMessage buyerMail = new MailMessage("OlexandrVeklych@gmail.com", buyerUser.Email)
                {
                    Subject = "BestLot.com",
                    Body = "Dear " + buyerUser.Name + " " + buyerUser.Surname + "," +
                    "Your bet was highest and now you can buy lot \"" + lotForSale.Name + "\"" +
                    "Please, contact seller on his email or telephone" +
                    "Email: " + sellerUser.Email +
                    "Telephone: " + sellerUser.TelephoneNumber
                };
                client.Send(buyerMail);


                MailMessage sellerMail = new MailMessage("OlexandrVeklych@gmail.com", sellerUser.Email)
                {
                    Subject = "BestLot.com",
                    Body = "Dear " + sellerUser.Name + " " + sellerUser.Surname + "," +
                    "Your lot \"" + lotForSale.Name + "\" was sold to " + buyerUser.Name + " " + buyerUser.Surname +
                    "Please, contact buyer on his email or telephone" +
                    "Email: " + buyerUser.Email +
                    "Telephone: " + buyerUser.TelephoneNumber
                };
                client.Send(sellerMail);
            }
                UoW.LotArchive.Add(UoW.Lots.Get(lotId));
                UoW.Lots.Delete(lotId);
                UoW.SaveArchiveChanges();
                UoW.SaveChanges();
            
            // throw new NotImplementedException(); //Implement sending Email
        }
    }
}
