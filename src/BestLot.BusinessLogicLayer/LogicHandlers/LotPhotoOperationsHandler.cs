﻿using System;
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
    public class LotPhotoOperationsHandler : ILotPhotosOperationsHandler
    {
        public LotPhotoOperationsHandler(IUnitOfWork unitOfWork)
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
            lotOperationsHandler = LogicDependencyResolver.ResloveLotOperationsHandler();
            userAccountOperationsHandler = LogicDependencyResolver.ResloveUserAccountOperationsHandler();
        }

        private ILotOperationsHandler lotOperationsHandler;
        private IUserAccountOperationsHandler userAccountOperationsHandler;
        private IUnitOfWork UoW;
        private IMapper mapper;

        public void AddPhotosToExistingLot(int lotId, LotPhoto[] lotPhotos, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            Lot lot = mapper.Map<Lot>(UoW.Lots.Get(lotId));
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
                UoW.LotPhotos.Add(mapper.Map<LotPhotoEntity>(lotPhotos[i]));
            }
            lotOperationsHandler.ChangeLotUnsafe(lot);
        }

        public async Task AddPhotosToExistingLotAsync(int lotId, LotPhoto[] lotPhotos, string hostingEnvironmentPath, string requestUriLeftPart)
        {
            Lot lot = mapper.Map<Lot>(await UoW.Lots.GetAsync(lotId));
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
                UoW.LotPhotos.Add(mapper.Map<LotPhotoEntity>(lotPhotos[i]));
            }
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

        public IQueryable<LotPhoto> GetLotPhotos(int lotId, params Expression<Func<Lot, object>>[] includeProperties)
        {
            Expression<Func<LotPhotoEntity, bool>> predicate = null;
            return UoW.LotPhotos.GetAll().Where(predicate = lotPhoto => lotPhoto.LotId == lotId).ProjectTo<LotPhoto>(mapper.ConfigurationProvider);
        }

        public async Task<IQueryable<LotPhoto>> GetLotPhotosAsync(int lotId, params Expression<Func<Lot, object>>[] includeProperties)
        {
            Expression<Func<LotPhotoEntity, bool>> predicate = null;
            return (await UoW.LotPhotos.GetAllAsync()).Where(predicate = lotPhoto => lotPhoto.LotId == lotId).ProjectTo<LotPhoto>(mapper.ConfigurationProvider);
        }
    }
}