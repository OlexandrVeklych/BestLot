using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using BestLot.BusinessLogicLayer.LogicHandlers;
using BestLot.BusinessLogicLayer.Models;
using BestLot.BusinessLogicLayer;
using BestLot.WebAPI.Models;
using AutoMapper;

namespace BestLot.WebAPI.Controllers
{
    public class LotCommentsController : ApiController
    {
        public LotCommentsController()
        {
            User.Identity.GetUserId();
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
        [Route("api/lots/{lotId}/comments")]
        public IHttpActionResult Get(int lotId)
        {
            try
            {
                return Ok(mapper.Map<LotModel>(lotOperationsHandler.GetLot(lotId, lot => lot.LotComments)).LotComments);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<controller>/5
        [Route("api/lots/{lotId}/comments/{commentNumber}")]
        public IHttpActionResult Get(int lotId, int commentNumber)
        {
            try
            {
                return Ok(mapper.Map<LotModel>(lotOperationsHandler.GetLot(lotId, lot => lot.LotComments)).LotComments[commentNumber]);
            }
            catch(IndexOutOfRangeException)
            {
                return BadRequest("Wrong number of comment");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<controller>
        [Route("api/lots/{lotId}/comments")]
        public IHttpActionResult Post([FromUri]int lotId, [FromBody]LotCommentModel value)
        {
            value.LotId = lotId;
            try
            {
                lotOperationsHandler.AddComment(mapper.Map<LotComment>(value));
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {

        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}