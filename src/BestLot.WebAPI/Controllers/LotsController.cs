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
                cfg.CreateMap<LotModel, Lot>();
                cfg.CreateMap<Lot, LotModel>();
                cfg.CreateMap<LotCommentModel, LotComment>();
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
            try
            {
                return Ok(mapper.Map<LotModel>(lotOperationsHandler.GetLot(id, lot => lot.LotComments, lot => lot.LotPhotos, lot => lot.SellerUser)));
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<controller>
        public IHttpActionResult Post([FromBody]LotModel value)
        {
            value.SellerUserId = User.Identity.Name;
            try
            {
                lotOperationsHandler.AddLot(mapper.Map<Lot>(value));
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }

        // PUT api/<controller>/5
        public IHttpActionResult Put(int id, [FromBody]LotModel value)
        {
            if (value.SellerUserId != User.Identity.Name)
                return BadRequest("Not allowed");
            try
            {
                lotOperationsHandler.ChangeLot(id, mapper.Map<Lot>(value));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }

        // DELETE api/<controller>/5
        public IHttpActionResult Delete(int id)
        {
            if (lotOperationsHandler.GetLot(id).SellerUserId != User.Identity.Name)
                return BadRequest("Not Allowed");
            try
            {
                lotOperationsHandler.DeleteLot(id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}