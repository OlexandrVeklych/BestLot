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

namespace BestLot.BusinessLogicLayer.LogicHandlers
{
    public class LotPhotoOperationsHandler : ILotPhotoOperationsHandler
    {
        private LotPhotoOperationsHandler()
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
        }

        public LotPhotoOperationsHandler(IUnitOfWork unitOfWork, ILotOperationsHandler lotOperationsHandler) : this()
        {
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

        public void AddPhotosToExistingLot(int lotId, LotPhoto[] lotPhotos, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            Lot lot = lotOperationsHandler.GetLot(lotId);
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

        public async Task AddPhotosToExistingLotAsync(int lotId, LotPhoto[] lotPhotos, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            Expression<Func<LotEntity, bool>> predicate = null;
            Lot lot = mapper.Map<Lot>((await UoW.Lots.GetAllAsync()).Where(predicate = l => l.Id == lotId).First());
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

        public void DeletePhoto(int photoId, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            LotPhoto lotPhoto;
            if ((lotPhoto = mapper.Map<LotPhoto>(UoW.LotPhotos.Get(photoId))) == null)
                throw new ArgumentException("Photo id is incorrect");
            File.Delete(lotPhoto.Path.Replace(requestUriLeftPart, hostingEnvironmentPath));
            UoW.LotPhotos.Delete(photoId);
            UoW.SaveChanges();
        }

        public async Task DeletePhotoAsync(int photoId, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            LotPhoto lotPhoto;
            if ((lotPhoto = mapper.Map<LotPhoto>(await UoW.LotPhotos.GetAsync(photoId))) == null)
                throw new ArgumentException("Photo id is incorrect");
            File.Delete(lotPhoto.Path.Replace(requestUriLeftPart, hostingEnvironmentPath));
            UoW.LotPhotos.Delete(photoId);
            await UoW.SaveChangesAsync();
        }

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

        public LotPhoto GetLotPhotoByNumber(int lotId, int photoNumber)
        {
            IQueryable<LotPhoto> lotPhotos = GetLotPhotos(lotId);
            if (photoNumber >= lotPhotos.Count())
                return null;
            return lotPhotos.ToList()[photoNumber];
        }

        public async Task<LotPhoto> GetLotPhotoByNumberAsync(int lotId, int photoNumber)
        {
            IQueryable<LotPhoto> lotPhotos = await GetLotPhotosAsync(lotId);
            if (photoNumber >= lotPhotos.Count())
                return null;
            return lotPhotos.ToList()[photoNumber];
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
    }
}
