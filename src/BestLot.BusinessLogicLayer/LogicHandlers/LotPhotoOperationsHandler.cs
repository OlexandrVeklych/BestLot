using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BestLot.BusinessLogicLayer.Models;
using BestLot.DataAccessLayer.Entities;
using BestLot.DataAccessLayer.UnitOfWork;
using BestLot.BusinessLogicLayer.Interfaces;
using BestLot.BusinessLogicLayer.Exceptions;

namespace BestLot.BusinessLogicLayer.LogicHandlers
{
    public class LotPhotoOperationsHandler : ILotPhotoOperationsHandler
    {
        public LotPhotoOperationsHandler(IUnitOfWork unitOfWork, ILotOperationsHandler lotOperationsHandler)
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LotEntity, Lot>()
                .ForAllMembers(opt => opt.Ignore());
                cfg.CreateMap<UserAccountInfoEntity, UserAccountInfo>()
                .ForAllMembers(opt => opt.Ignore());
                cfg.CreateMap<LotPhotoEntity, LotPhoto>()
                .ForMember(dest => dest.Lot, opt => opt.Ignore());
                cfg.CreateMap<LotCommentEntity, LotComment>()
                .ForAllMembers(opt => opt.Ignore());

                cfg.CreateMap<LotPhoto, LotPhotoEntity>();
            }).CreateMapper();
            this.UoW = unitOfWork;
            this.lotOperationsHandler = lotOperationsHandler;
        }

        private ILotOperationsHandler lotOperationsHandler;
        public ILotOperationsHandler LotOperationsHandler
        {
            get
            {
                return lotOperationsHandler;
            }
            set
            {
                lotOperationsHandler = value;
            }
        }

        private IUnitOfWork UoW;
        private IMapper mapper;

        //hostingEnvironmentPath - physical path to WebAPI folder
        //requestUriLeftPart - URL
        public void AddPhotosToExistingLot(int lotId, LotPhoto[] lotPhotos, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            Lot lot = lotOperationsHandler.GetLot(lotId);
            lot.LotPhotos = GetLotPhotos(lotId).ToList();
            if (lot.LotPhotos.Count() + lotPhotos.Count() > 10)
                throw new WrongModelException("Maximum 10 photos per lot, currently this lot has " + lot.LotPhotos.Count());
            string currentDirectory = hostingEnvironmentPath + "\\Photos\\" + lot.SellerUserId;
            if (!Directory.Exists(currentDirectory))
                Directory.CreateDirectory(currentDirectory);
            for (int i = 0; i < lotPhotos.Length; i++)
            {
                byte[] photoBytes = Convert.FromBase64String(lotPhotos[i].Path);
                string photoPath = currentDirectory + "\\" + lot.Name + "_" + DateTime.Now.ToFileTime() + ".jpeg";
                File.WriteAllBytes(photoPath, photoBytes);
                lotPhotos[i].Path = photoPath.Replace(hostingEnvironmentPath, requestUriLeftPart);
                lotPhotos[i].LotId = lot.Id;
                lot.AddPhoto(lotPhotos[i]);
                UoW.LotPhotos.Add(mapper.Map<LotPhotoEntity>(lotPhotos[i]));
            }
            UoW.SaveChanges();
            lotOperationsHandler.ChangeLotUnsafe(lot);
        }

        //hostingEnvironmentPath - physical path to WebAPI folder
        //requestUriLeftPart - URL
        public async Task AddPhotosToExistingLotAsync(int lotId, LotPhoto[] lotPhotos, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            Lot lot = LotOperationsHandler.GetLot(lotId);
            lot.LotPhotos = (await GetLotPhotosAsync(lotId)).ToList();
            if (lot.LotPhotos.Count() + lotPhotos.Count() > 10)
                throw new WrongModelException("Maximum 10 photos per lot, currently this lot has " + lot.LotPhotos.Count());
            string currentDirectory = hostingEnvironmentPath + "\\Photos\\" + lot.SellerUserId;
            if (!Directory.Exists(currentDirectory))
                Directory.CreateDirectory(currentDirectory);
            for (int i = 0; i < lotPhotos.Length; i++)
            {
                byte[] photoBytes = Convert.FromBase64String(lotPhotos[i].Path);
                string photoPath = currentDirectory + "\\" + lot.Name + "_" + DateTime.Now.ToFileTime() + ".jpeg";
                File.WriteAllBytes(photoPath, photoBytes);
                lotPhotos[i].Path = photoPath.Replace(hostingEnvironmentPath, requestUriLeftPart);
                lot.AddPhoto(lotPhotos[i]);
                lotPhotos[i].LotId = lot.Id;
                UoW.LotPhotos.Add(mapper.Map<LotPhotoEntity>(lotPhotos[i]));
            }
            await UoW.SaveChangesAsync();
            await lotOperationsHandler.ChangeLotUnsafeAsync(lot);
        }

        //hostingEnvironmentPath - physical path to WebAPI folder
        //requestUriLeftPart - URL
        public void AddPhotosToNewLot(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            string currentDirectory = hostingEnvironmentPath + "\\Photos\\" + lot.SellerUserId;
            if (!Directory.Exists(currentDirectory))
                Directory.CreateDirectory(currentDirectory);
            for (int i = 0; i < lot.LotPhotos.Count; i++)
            {
                byte[] photoBytes = Convert.FromBase64String(lot.LotPhotos[i].Path);
                string photoPath = currentDirectory + "\\" + lot.Name + "_" + DateTime.Now.ToFileTime() + ".jpeg";
                File.WriteAllBytes(photoPath, photoBytes);
                lot.LotPhotos[i].Path = photoPath.Replace(hostingEnvironmentPath, requestUriLeftPart);
            }
        }

        //hostingEnvironmentPath - physical path to WebAPI folder
        //requestUriLeftPart - URL
        public async Task AddPhotosToNewLotAsync(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            await new Task(() =>
            {
                string currentDirectory = hostingEnvironmentPath + "\\Photos\\" + lot.SellerUserId;
                if (!Directory.Exists(currentDirectory))
                    Directory.CreateDirectory(currentDirectory);
                for (int i = 0; i < lot.LotPhotos.Count; i++)
                {
                    byte[] photoBytes = Convert.FromBase64String(lot.LotPhotos[i].Path);
                    string photoPath = currentDirectory + "\\" + lot.Name + "_" + DateTime.Now.ToFileTime() + ".jpeg";
                    File.WriteAllBytes(photoPath, photoBytes);
                    lot.LotPhotos[i].Path = photoPath.Replace(hostingEnvironmentPath, requestUriLeftPart);
                }
            });
        }

        //hostingEnvironmentPath - physical path to WebAPI folder
        //requestUriLeftPart - URL
        public void DeletePhoto(int photoId, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            LotPhoto lotPhoto;
            if ((lotPhoto = mapper.Map<LotPhoto>(UoW.LotPhotos.Get(photoId))) == null)
                throw new ArgumentException("Photo id is incorrect");
            File.Delete(lotPhoto.Path.Replace(requestUriLeftPart, hostingEnvironmentPath));
            UoW.LotPhotos.Delete(photoId);
            UoW.SaveChanges();
        }

        //hostingEnvironmentPath - physical path to WebAPI folder
        //requestUriLeftPart - URL
        public async Task DeletePhotoAsync(int photoId, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            LotPhoto lotPhoto;
            if ((lotPhoto = mapper.Map<LotPhoto>(await UoW.LotPhotos.GetAsync(photoId))) == null)
                throw new ArgumentException("Photo id is incorrect");
            File.Delete(lotPhoto.Path.Replace(requestUriLeftPart, hostingEnvironmentPath));
            UoW.LotPhotos.Delete(photoId);
            await UoW.SaveChangesAsync();
        }

        //hostingEnvironmentPath - physical path to WebAPI folder
        //requestUriLeftPart - URL
        public void DeleteAllLotPhotos(int lotId, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            Expression<Func<LotPhotoEntity, bool>> predicate = null;
            List<LotPhoto> lotPhotos = UoW.LotPhotos
                .GetAll()
                .Where(predicate = photo => photo.LotId == lotId)
                .ProjectTo<LotPhoto>(mapper.ConfigurationProvider)
                .ToList();
            foreach (LotPhoto lotPhoto in lotPhotos)
            {
                File.Delete(lotPhoto.Path.Replace(requestUriLeftPart, hostingEnvironmentPath));
                UoW.LotPhotos.Delete(lotPhoto.Id);
            }
            UoW.SaveChanges();
        }

        //hostingEnvironmentPath - physical path to WebAPI folder
        //requestUriLeftPart - URL
        public async Task DeleteAllLotPhotosAsync(int lotId, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            Expression<Func<LotPhotoEntity, bool>> predicate = null;
            List<LotPhoto> lotPhotos = (await UoW.LotPhotos
                .GetAllAsync())
                .Where(predicate = photo => photo.LotId == lotId)
                .ProjectTo<LotPhoto>(mapper.ConfigurationProvider)
                .ToList();
            foreach (LotPhoto lotPhoto in lotPhotos)
            {
                File.Delete(lotPhoto.Path.Replace(requestUriLeftPart, hostingEnvironmentPath));
                UoW.LotPhotos.Delete(lotPhoto.Id);
            }
            await UoW.SaveChangesAsync();
        }

        //hostingEnvironmentPath - physical path to WebAPI folder
        //requestUriLeftPart - URL
        public void DeleteAllUserPhotos(string userAccountId, string hostingEnvironmentPath)
        {
            if (Directory.Exists(hostingEnvironmentPath + "\\Photos\\" + userAccountId))
                Directory.Delete(hostingEnvironmentPath + "\\Photos\\" + userAccountId, true);
        }

        public async Task DeleteAllUserPhotosAsync(string userAccountId, string hostingEnvironmentPath)
        {
            if (Directory.Exists(hostingEnvironmentPath + "\\Photos\\" + userAccountId))
                await new Task(() =>
                {
                    Directory.Delete(hostingEnvironmentPath + "\\Photos\\" + userAccountId, true);
                });
        }

        public LotPhoto GetLotPhotoByPosition(int lotId, int photoPosition)
        {
            //This will throw exception if lotId is wrong
            List<LotPhoto> lotPhotos = GetLotPhotos(lotId).ToList();
            if (lotPhotos.Count() <= photoPosition)
                throw new WrongModelException("No photo on that position");
            return lotPhotos[photoPosition];
        }

        public async Task<LotPhoto> GetLotPhotoByPositionAsync(int lotId, int photoPosition)
        {
            //This will throw exception if lotId is wrong
            List<LotPhoto> lotPhotos = (await GetLotPhotosAsync(lotId)).ToList();
            if (lotPhotos.Count() <= photoPosition)
                throw new WrongModelException("No photo on that position");
            return lotPhotos[photoPosition];
        }

        public IQueryable<LotPhoto> GetLotPhotos(int lotId)
        {
            //This will throw exception if lotId is wrong
            lotOperationsHandler.GetLot(lotId);
            Expression<Func<LotPhotoEntity, bool>> predicate = null;
            return UoW.LotPhotos.GetAll()
                .Where(predicate = lotPhoto => lotPhoto.LotId == lotId)
                .ProjectTo<LotPhoto>(mapper.ConfigurationProvider);
        }

        public async Task<IQueryable<LotPhoto>> GetLotPhotosAsync(int lotId)
        {
            //This will throw exception if lotId is wrong
            await lotOperationsHandler.GetLotAsync(lotId);
            Expression<Func<LotPhotoEntity, bool>> predicate = null;
            return (await UoW.LotPhotos.GetAllAsync())
                .Where(predicate = lotPhoto => lotPhoto.LotId == lotId)
                .ProjectTo<LotPhoto>(mapper.ConfigurationProvider);
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                UoW.Dispose();
                if (disposing)
                {
                    lotOperationsHandler = null;
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
