using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.Models
{
    public class Lot
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string SellerUserId { get; set; }
        public UserAccountInfo SellerUser { get; set; }
        public string BuyerUserId { get; set; }
        public double Price { get; set; }
        public double MinStep { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime SellDate { get; set; }
        public List<LotPhoto> LotPhotos { get; set; }
        public List<LotComment> LotComments { get; set; }

        public void AddComment(LotComment lotComment)
        {
            LotComments.Add(lotComment);
        }

        public void AddPhoto(LotPhoto lotPhoto)
        {
            LotPhotos.Add(lotPhoto);
        }

        public void Sell(UserAccountInfo buyerUser)
        {
            if (buyerUser != null)
            {
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
                    "Your bet was highest and now you can buy lot \"" + Name + "\"" +
                    "Please, contact seller on his email or telephone" +
                    "Email: " + SellerUser.Email +
                    "Telephone: " + SellerUser.TelephoneNumber
                };
                client.Send(buyerMail);


                MailMessage sellerMail = new MailMessage("OlexandrVeklych@gmail.com", SellerUser.Email)
                {
                    Subject = "BestLot.com",
                    Body = "Dear " + SellerUser.Name + " " + SellerUser.Surname + "," +
                    "Your lot \"" + Name + "\" was sold to " + buyerUser.Name + " " + buyerUser.Surname +
                    "Please, contact buyer on his email or telephone" +
                    "Email: " + buyerUser.Email +
                    "Telephone: " + buyerUser.TelephoneNumber
                };
                client.Send(sellerMail);
            }
        }
    }
}
