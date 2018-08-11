﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using BestLot.BusinessLogicLayer.BidLogic;

namespace BestLot.BusinessLogicLayer.Models
{
    public class Lot : IBidable
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
        public int BidPlacer { get; set; }

        public void AddComment(LotComment lotComment)
        {
            LotComments.Add(lotComment);
        }

        public void AddPhoto(LotPhoto lotPhoto)
        {
            LotPhotos.Add(lotPhoto);
        }

        public virtual void PlaceBid(string buyerUserId, double price)
        {
            IBidPlacer bidPlacer = null;
            switch (BidPlacer)
            {
                case 1:
                    bidPlacer = new DeterminedSelldateBidPlacer();
                    break;
                case 2:
                    bidPlacer = new RelativeSelldateBidPlacer();
                    break;
                default:
                    bidPlacer = new DeterminedSelldateBidPlacer();
                    break;
            }
            bidPlacer.PlaceBid(this, buyerUserId, price);
        }

        public virtual void Sell(UserAccountInfo buyerUser)
        {
            if (buyerUser != null)
            {
                NetworkCredential login = new NetworkCredential("veklich99@gmail.com", "Veklich1999");
                SmtpClient client = new SmtpClient()
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = login
                };


                MailMessage buyerMail = new MailMessage(new MailAddress("veklich99@gmail.com", "BestLot", Encoding.UTF8), new MailAddress(buyerUser.Email))
                {
                    Subject = "BestLot.com",
                    Body = "Dear " + buyerUser.Name + " " + buyerUser.Surname + "\n" +
                    "Your bet was highest and now you can buy lot \"" + Name + "\" for " + Price + "\n" +
                    "Please, contact seller on his email or telephone\n" +
                    "Email: " + SellerUser.Email + "\n" +
                    "Telephone: " + SellerUser.TelephoneNumber,
                    BodyEncoding = UTF8Encoding.UTF8,
                    DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure
                };
                client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
                client.Send(buyerMail);


                MailMessage sellerMail = new MailMessage(new MailAddress("veklich99@gmail.com", "BestLot", Encoding.UTF8), new MailAddress(SellerUser.Email))
                {
                    Subject = "BestLot.com",
                    Body = "Dear " + SellerUser.Name + " " + SellerUser.Surname + ",\n" +
                    "Your lot \"" + Name + "\" was sold to " + buyerUser.Name + " " + buyerUser.Surname + " for " + Price + "\n" +
                    "Please, contact buyer on his email or telephone\n" +
                    "Email: " + buyerUser.Email + "\n" +
                    "Telephone: " + buyerUser.TelephoneNumber,
                    BodyEncoding = UTF8Encoding.UTF8,
                    DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure
                };
                client.Send(sellerMail);
            }
        }

        private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
                throw new ArgumentException("Sending cancelled");
            if (e.Error != null)
                throw new ArgumentException(e.Error.Message);
        }
    }
}
