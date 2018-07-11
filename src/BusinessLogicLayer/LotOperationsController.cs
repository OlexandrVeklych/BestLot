using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.UnitOfWork;
using DataAccessLayer.Entities;
using BusinessLogicLayer.Models;
using AutoMapper;
using System.Linq.Expressions;

namespace BusinessLogicLayer
{
    public class LotOperationsController
    {
        public LotOperationsController(IUnitOfWork unitOfWork)
        {
            UoW = unitOfWork;
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Lot, LotModel>();
                cfg.CreateMap<LotModel, Lot>();
                cfg.CreateMap<LotComment, LotCommentModel>();
                cfg.CreateMap<LotCommentModel, LotComment>();
                cfg.CreateMap<LotPhoto, LotPhotoModel>();
                cfg.CreateMap<LotPhotoModel, LotPhoto>();
                cfg.CreateMap<Expression<Func<LotModel, object>>, Expression<Func<Lot, object>>>();
                cfg.CreateMap<Func<LotModel, bool>, Func<Lot, bool>>();
            }).CreateMapper();
        }

        private IUnitOfWork UoW;
        private IMapper mapper;

        public void AddLot(LotModel lot)
        {
            UoW.Lots.Add(mapper.Map<Lot>(lot));
            UoW.SaveChanges();
        }

        public void ChangeLot(int lotId, LotModel newLot)
        {
            UoW.Lots.Modify(lotId, mapper.Map<Lot>(newLot));
            UoW.SaveChanges();
        }

        public void DeleteLot(int lotId)
        {
            UoW.Lots.Delete(lotId);
            UoW.SaveChanges();
        }

        public LotModel GetLot(int lotId)
        {
            return mapper.Map<LotModel>(UoW.Lots.Get(lotId));
        }

        public IEnumerable<LotModel> GetAllLots()
        {
            return mapper.Map<IEnumerable<LotModel>>(UoW.Lots.GetAll());
        }

        public IEnumerable<LotModel> GetAllLots(params Expression<Func<LotModel, object>>[] includeProperties)
        {
            return mapper.Map<IEnumerable<LotModel>>(UoW.Lots.GetAll(mapper.Map<Expression<Func<Lot, object>>>(includeProperties)));
        }

        public IQueryable<LotModel> GetAllLots(Func<LotModel, bool> predicate, params Expression<Func<LotModel, object>>[] includeProperties)
        {
            return mapper.Map<IQueryable<LotModel>>(UoW.Lots.GetAll(mapper.Map<Expression<Func<Lot, object>>>(includeProperties)).Where(mapper.Map<Func<Lot, bool>>(predicate)));

        }

        public void PlaceBet(int userId, int lotId, double price)
        {
            LotModel lotModel = mapper.Map<LotModel>(UoW.Lots.Get(lotId, lot => lot.LotPhotos, lot => lot.Cooments));
            lotModel.BuyerUserId = userId;
            lotModel.Price = price;

            UoW.Lots.Modify(lotId, mapper.Map<Lot>(lotModel));

            //UoW.Lots.Delete(lotModel.Id);
            //UoW.Lots.Add(mapper.Map<Lot>(lotModel));
            UoW.SaveChanges();
        }
    }
}
