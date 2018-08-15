﻿using BestLot.BusinessLogicLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.BusinessLogicLayer.Interfaces
{
    public interface ILotPhotoOperationsHandler : IDisposable
    {
        //For dependency injection into property
        ILotOperationsHandler LotOperationsHandler { get; set; }
        void AddPhotosToExistingLot(int lotId, LotPhoto[] lotPhotos, string hostingEnvironmentPath, string requestUriLeftPart);
        Task AddPhotosToExistingLotAsync(int lotId, LotPhoto[] lotPhotos, string hostingEnvironmentPath, string requestUriLeftPart);
        void AddPhotosToNewLot(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart);
        Task AddPhotosToNewLotAsync(Lot lot, string hostingEnvironmentPath, string requestUriLeftPart);
        void DeletePhoto(int photoId, string hostingEnvironmentPath, string requestUriLeftPart);
        Task DeletePhotoAsync(int photoId, string hostingEnvironmentPath, string requestUriLeftPart);
        void DeleteAllLotPhotos(int lotId, string hostingEnvironmentPath, string requestUriLeftPart);
        Task DeleteAllLotPhotosAsync(int lotId, string hostingEnvironmentPath, string requestUriLeftPart);
        void DeleteAllUserPhotos(string userAccountId, string hostingEnvironmentPath);
        Task DeleteAllUserPhotosAsync(string userAccountId, string hostingEnvironmentPath);
        LotPhoto GetLotPhotoByNumber(int lotId, int photoNumber);
        Task<LotPhoto> GetLotPhotoByNumberAsync(int lotId, int photoNumber);
        IQueryable<LotPhoto> GetLotPhotos(int lotId);
        Task<IQueryable<LotPhoto>> GetLotPhotosAsync(int lotId);
    }
}
