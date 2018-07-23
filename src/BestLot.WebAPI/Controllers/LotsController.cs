using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BestLot.BusinessLogicLayer.LogicHandlers;
using BestLot.BusinessLogicLayer.Models;
using BestLot.BusinessLogicLayer;
using BestLot.WebAPI.Models;
using AutoMapper;

namespace BestLot.WebAPI.Controllers
{
    public class LotsController : ApiController
    {
        public LotsController()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LotModel, Lot>().MaxDepth(1);
                cfg.CreateMap<Lot, LotModel>();
                cfg.CreateMap<LotCommentModel, LotComment>().MaxDepth(1);
                cfg.CreateMap<LotComment, LotCommentModel>();
                cfg.CreateMap<LotPhotoModel, LotPhoto>();
                cfg.CreateMap<LotPhoto, LotPhotoModel>();
            }).CreateMapper();
            lotOperationsHandler = LogicDependencyResolver.ResloveLotOperationsHandler();
        }

        private readonly ILotOperationsHandler lotOperationsHandler;
        private readonly IMapper mapper;

        // GET api/<controller>
        public IHttpActionResult Get()
        {
            return Ok(mapper.Map<IEnumerable<LotModel>>(lotOperationsHandler.GetAllLots(lot => lot.LotComments, lot => lot.LotPhotos, lot => lot.SellerUser)));
        }

        // GET api/<controller>/5
        public IHttpActionResult Get(int id)
        {
            return Ok(mapper.Map<LotModel>(lotOperationsHandler.GetLot(id, lot => lot.LotComments, lot => lot.LotPhotos, lot => lot.SellerUser)));
        }

        // POST api/<controller>
        public void Post([FromBody]LotModel value)
        {
            lotOperationsHandler.AddLot(mapper.Map<Lot>(value));
        }

        // PUT api/<controller>/5
        public IHttpActionResult Put(int id, [FromBody]LotModel value)
        {
            try
            {
                lotOperationsHandler.ChangeLot(id, mapper.Map<Lot>(value));
                return Ok();
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/<controller>/5
        public IHttpActionResult Delete(int id)
        {
            try
            {
                lotOperationsHandler.DeleteLot(id);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}