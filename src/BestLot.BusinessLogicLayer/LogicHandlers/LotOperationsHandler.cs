using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestLot.DataAccessLayer.UnitOfWork;
using BestLot.DataAccessLayer.Entities;
using BestLot.BusinessLogicLayer.Models;
using AutoMapper;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using BestLot.BusinessLogicLayer.Interfaces;

namespace BestLot.BusinessLogicLayer.LogicHandlers
{
    public class LotOperationsHandler : ILotOperationsHandler
    {
        public LotOperationsHandler(IUnitOfWork unitOfWork)
        {
            UoW = unitOfWork;
            mapper = new MapperConfiguration(cfg =>
            {
                //MaxDepth(1) - to map User inside Lot without his Lots
                //Lot.User.Name - ok
                //Lot.User.Lots[n] - null reference
                cfg.CreateMap<LotEntity, Lot>().MaxDepth(1);
                cfg.CreateMap<Lot, LotEntity>();
                //MaxDepth(1) - to map User inside Comment without his Lots
                //LotComment.User.Name - ok
                //LotComment.User.Lots[n] - null reference
                cfg.CreateMap<LotCommentEntity, LotComment>().MaxDepth(1);
                cfg.CreateMap<LotComment, LotCommentEntity>();
                cfg.CreateMap<LotPhotoEntity, LotPhoto>();
                cfg.CreateMap<LotPhoto, LotPhotoEntity>();
                cfg.CreateMap<UserAccountInfo, UserAccountInfoEntity>();
                cfg.CreateMap<UserAccountInfoEntity, UserAccountInfo>();
                cfg.CreateMap<Expression<Func<Lot, object>>[], Expression<Func<LotEntity, object>>[]>();
            }).CreateMapper();
            lotPhotosOperationsHandler = LogicDependencyResolver.ResloveLotPhotosOperationsHandler();
        }
        private ILotPhotosOperationsHandler lotPhotosOperationsHandler;
        private IUnitOfWork UoW;
        private IMapper mapper;

        public void AddLot(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            if (UoW.UserAccounts.Get(lot.SellerUserId) == null)
                throw new ArgumentException("Seller user id is incorrect");
            lot.StartDate = DateTime.Now;
            lot.BuyerUserId = null;
            lotPhotosOperationsHandler.AddPhotosToNewLot(lot, hostingEnvironmentPath, requestUriLeftPart);
            UoW.Lots.Add(mapper.Map<LotEntity>(lot));
            UoW.SaveChanges();
        }

        public async Task AddLotAsync(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            if (await UoW.UserAccounts.GetAsync(lot.SellerUserId) == null)
                throw new ArgumentException("Seller user id is incorrect");
            lot.StartDate = DateTime.Now;
            lot.BuyerUserId = null;
            await lotPhotosOperationsHandler.AddPhotosToNewLotAsync(lot, hostingEnvironmentPath, requestUriLeftPart);
            UoW.Lots.Add(mapper.Map<LotEntity>(lot));
            await UoW.SaveChangesAsync();
        }

        public void ChangeLot(int id, Lot newLot, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            if (UoW.Lots.Get(id) == null)
                AddLot(newLot, hostingEnvironmentPath, requestUriLeftPart);
            else
            {
                Lot currentLot = mapper.Map<Lot>(UoW.Lots.Get(id));
                if (currentLot.Id != newLot.Id
                    || (currentLot.BuyerUserId != null && currentLot.Price != newLot.Price)
                    || currentLot.SellerUserId != newLot.SellerUserId
                    || currentLot.BuyerUserId != newLot.BuyerUserId)
                    throw new ArgumentException("No permission to change these properties");
                UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
                UoW.SaveChanges();
            }
        }

        public async Task ChangeLotAsync(int id, Lot newLot, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            if (await UoW.Lots.GetAsync(id) == null)
                await AddLotAsync(newLot, hostingEnvironmentPath, requestUriLeftPart);
            else
            {
                Lot currentLot = mapper.Map<Lot>(await UoW.Lots.GetAsync(id));
                if (currentLot.Id != newLot.Id
                    || (currentLot.BuyerUserId != null && currentLot.Price != newLot.Price)
                    || currentLot.SellerUserId != newLot.SellerUserId
                    || currentLot.BuyerUserId != newLot.BuyerUserId)
                    throw new ArgumentException("No permission to change these properties");
                UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
                await UoW.SaveChangesAsync();
            }
        }

        //id of newLot is correct, don`t check it again
        public void ChangeLotUnsafe(Lot newLot)
        {
            UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
            UoW.SaveChanges();
        }

        //id of newLot is correct, don`t check it again
        public async Task ChangeLotUnsafeAsync(Lot newLot)
        {
            UoW.Lots.Modify(newLot.Id, mapper.Map<LotEntity>(newLot));
            await UoW.SaveChangesAsync();
        }

        public void DeleteLot(int lotId, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            Lot lot;
            if ((lot = mapper.Map<Lot>(UoW.Lots.Get(lotId))) == null)
                throw new ArgumentException("Lot id is incorrect");
            foreach(LotPhoto lotPhoto in lot.LotPhotos)
                lotPhotosOperationsHandler.DeletePhoto(lotPhoto.Id, hostingEnvironmentPath, requestUriLeftPart);
            UoW.Lots.Delete(lotId);
            UoW.SaveChanges();
        }

        public async Task DeleteLotAsync(int lotId, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            Lot lot;
            if ((lot = mapper.Map<Lot>(await UoW.Lots.GetAsync(lotId))) == null)
                throw new ArgumentException("Lot id is incorrect");
            foreach (LotPhoto lotPhoto in lot.LotPhotos)
                await lotPhotosOperationsHandler.DeletePhotoAsync(lotPhoto.Id, hostingEnvironmentPath, requestUriLeftPart);
            UoW.Lots.Delete(lotId);
            await UoW.SaveChangesAsync();
        }
        
        //private void DeleteLotPhotos(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart)
        //{
        //    foreach(LotPhoto lotPhoto in lot.LotPhotos)
        //    {
        //        if (lotPhoto.Path.Contains(requestUriLeftPart))
        //            File.Delete(lotPhoto.Path.Replace(requestUriLeftPart, hostingEnvironmentPath));
        //        else
        //            File.Delete(lotPhoto.Path);
        //    }
        //}
        //
        //private async Task DeleteLotPhotosAsync(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart)
        //{
        //    await new Task(() =>
        //    {
        //        foreach (LotPhoto lotPhoto in lot.LotPhotos)
        //        {
        //            if (lotPhoto.Path.Contains(requestUriLeftPart))
        //                File.Delete(lotPhoto.Path.Replace(requestUriLeftPart, hostingEnvironmentPath));
        //            else
        //                File.Delete(lotPhoto.Path);
        //        }
        //    });
        //}

        public Lot GetLot(int lotId, params Expression<Func<Lot, object>>[] includeProperties)
        {
            Lot lot = mapper.Map<Lot>(UoW.Lots.Get(lotId, mapper.Map<Expression<Func<LotEntity, object>>[]>(includeProperties)));
            if (lot == null)
                throw new ArgumentException("Lot id is incorrect");
            return lot;
        }

        public async Task<Lot> GetLotAsync(int lotId, params Expression<Func<Lot, object>>[] includeProperties)
        {
            Lot lot = mapper.Map<Lot>(await UoW.Lots.GetAsync(lotId, mapper.Map<Expression<Func<LotEntity, object>>[]>(includeProperties)));
            if (lot == null)
                throw new ArgumentException("Lot id is incorrect");
            return lot;
        }

        public IQueryable<Lot> GetAllLots(params Expression<Func<Lot, object>>[] includeProperties)
        {
            return UoW.Lots.GetAll(mapper.Map<Expression<Func<LotEntity, object>>[]>(includeProperties)).ProjectTo<Lot>(mapper.ConfigurationProvider);
        }

        public async Task<IQueryable<Lot>> GetAllLotsAsync(params Expression<Func<Lot, object>>[] includeProperties)
        {
            var result = await UoW.Lots.GetAllAsync(mapper.Map<Expression<Func<LotEntity, object>>[]>(includeProperties));
            return result.ProjectTo<Lot>(mapper.ConfigurationProvider);
        }

        public void PlaceBet(string buyerUserId, int lotId, double price)
        {
            if (UoW.UserAccounts.Get(buyerUserId) == null)
                throw new ArgumentException("User id is incorrect");
            Lot lot = mapper.Map<Lot>(UoW.Lots.Get(lotId, l => l.LotPhotos, l => l.LotComments, l => l.SellerUser));
            if (lot == null)
                throw new ArgumentException("Lot id is incorrect");
            if (price < lot.Price + lot.MinStep)
                throw new ArgumentException("Your bet can be " + (lot.Price + lot.MinStep) + " or higher");
            lot.BuyerUserId = buyerUserId;
            lot.Price = price;

            //private ChangeLot, without checking LotId
            ChangeLotUnsafe(lot);
        }

        public async Task PlaceBetAsync(string buyerUserId, int lotId, double price)
        {
            if (await UoW.UserAccounts.GetAsync(buyerUserId) == null)
                throw new ArgumentException("User id is incorrect");
            Lot lot = mapper.Map<Lot>(await UoW.Lots.GetAsync(lotId, l => l.LotPhotos, l => l.LotComments, l => l.SellerUser));
            if (lot == null)
                throw new ArgumentException("Lot id is incorrect");
            if (price < lot.Price + lot.MinStep)
                throw new ArgumentException("Your bet can be " + (lot.Price + lot.MinStep) + " or higher");
            lot.BuyerUserId = buyerUserId;
            lot.Price = price;

            //private ChangeLot, without checking LotId
            await ChangeLotUnsafeAsync(lot);
        }

        //public void AddPhotos(int lotId, LotPhoto[] lotPhotos, string hostingEnvironmentPath, string requestUriLeftPart)
        //{
        //    Lot lot = mapper.Map<Lot>(UoW.Lots.Get(lotId));
        //    string currentDirectory = hostingEnvironmentPath + "\\Photos\\" + lot.SellerUserId;
        //    if (!Directory.Exists(currentDirectory))
        //        Directory.CreateDirectory(currentDirectory);
        //    for (int i = 0; i < lotPhotos.Length; i++)
        //    {
        //        byte[] photoBytes = Convert.FromBase64String(lotPhotos[i].Path);
        //        string photoPath = currentDirectory + "\\" + lot.Name + "_" + DateTime.Now.ToFileTime() + ".jpeg";
        //        File.WriteAllBytes(photoPath, photoBytes);
        //        lotPhotos[i].Path = photoPath.Replace(hostingEnvironmentPath, requestUriLeftPart);
        //        lot.AddPhoto(lotPhotos[i]);
        //        UoW.LotPhotos.Add(mapper.Map<LotPhotoEntity>(lotPhotos[i]));
        //    }
        //    ChangeLotUnsafe(lot);
        //}
        //
        //public async Task AddPhotosAsync(int lotId, LotPhoto[] lotPhotos, string hostingEnvironmentPath, string requestUriLeftPart)
        //{
        //    Lot lot = mapper.Map<Lot>(await UoW.Lots.GetAsync(lotId));
        //    string currentDirectory = hostingEnvironmentPath + "\\Photos\\" + lot.SellerUserId;
        //    if (!Directory.Exists(currentDirectory))
        //        Directory.CreateDirectory(currentDirectory);
        //    for (int i = 0; i < lotPhotos.Length; i++)
        //    {
        //        byte[] photoBytes = Convert.FromBase64String(lotPhotos[i].Path);
        //        string photoPath = currentDirectory + "\\" + lot.Name + "_" + DateTime.Now.ToFileTime() + ".jpeg";
        //        File.WriteAllBytes(photoPath, photoBytes);
        //        lotPhotos[i].Path = photoPath.Replace(hostingEnvironmentPath, requestUriLeftPart);
        //        lot.AddPhoto(lotPhotos[i]);
        //        UoW.LotPhotos.Add(mapper.Map<LotPhotoEntity>(lotPhotos[i]));
        //    }
        //    await ChangeLotUnsafeAsync(lot);
        //}
        //
        //private void AddPhotos(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart)
        //{
        //    string currentDirectory = hostingEnvironmentPath + "\\Photos\\" + lot.SellerUserId;
        //    if (!Directory.Exists(currentDirectory))
        //        Directory.CreateDirectory(currentDirectory);
        //    for (int i = 0; i < lot.LotPhotos.Count; i++)
        //    {
        //        byte[] photoBytes = Convert.FromBase64String(lot.LotPhotos[i].Path);
        //        string photoPath = currentDirectory + "\\" + lot.Name + "_" + DateTime.Now.ToFileTime() + ".jpeg";
        //        File.WriteAllBytes(photoPath, photoBytes);
        //        lot.LotPhotos[i].Path = photoPath.Replace(hostingEnvironmentPath, requestUriLeftPart);
        //    }
        //}
        //
        //private async Task AddPhotosAsync(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart)
        //{
        //    await new Task(() =>
        //    {
        //        string currentDirectory = hostingEnvironmentPath + "\\Photos\\" + lot.SellerUserId;
        //        if (!Directory.Exists(currentDirectory))
        //            Directory.CreateDirectory(currentDirectory);
        //        for (int i = 0; i < lot.LotPhotos.Count; i++)
        //        {
        //            byte[] photoBytes = Convert.FromBase64String(lot.LotPhotos[i].Path);
        //            string photoPath = currentDirectory + "\\" + lot.Name + "_" + DateTime.Now.ToFileTime() + ".jpeg";
        //            File.WriteAllBytes(photoPath, photoBytes);
        //            lot.LotPhotos[i].Path = photoPath.Replace(hostingEnvironmentPath, requestUriLeftPart);
        //        }
        //    });
        //}
        //
        //public void DeletePhoto(int photoId, string hostingEnvironmentPath, string requestUriLeftPart)
        //{
        //    LotPhoto lotPhoto;
        //    if ((lotPhoto = mapper.Map<LotPhoto>(UoW.LotPhotos.Get(photoId))) == null)
        //        throw new ArgumentException("Photo id is incorrect");
        //    File.Delete(lotPhoto.Path.Replace(requestUriLeftPart, hostingEnvironmentPath));
        //    UoW.LotPhotos.Delete(photoId);
        //    UoW.SaveChanges();
        //}
        //
        //public async Task DeletePhotoAsync(int photoId, string hostingEnvironmentPath, string requestUriLeftPart)
        //{
        //    LotPhoto lotPhoto;
        //    if ((lotPhoto = mapper.Map<LotPhoto>(await UoW.LotPhotos.GetAsync(photoId))) == null)
        //        throw new ArgumentException("Photo id is incorrect");
        //    File.Delete(lotPhoto.Path.Replace(requestUriLeftPart, hostingEnvironmentPath));
        //    UoW.LotPhotos.Delete(photoId);
        //    await UoW.SaveChangesAsync();
        //}
    }
}
