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
            userAccountOperationsHandler = LogicDependencyResolver.ResloveUserAccountOperationsHandler();
        }

        private readonly ILotOperationsHandler lotOperationsHandler;
        private readonly IUserAccountOperationsHandler userAccountOperationsHandler;
        private readonly IMapper mapper;
        // GET api/<controller>
        [Route("api/lots/{lotId}/comments")]
        public IHttpActionResult Get(int lotId, int page, int amount)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<LotCommentModel>>(lotOperationsHandler.GetLot(lotId, lot => lot.LotComments).LotComments.Skip((page - 1) * amount).Take(amount)));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [Route("api/users/{email}/comments")]
        public IHttpActionResult Get(string email, int page, int amount)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<LotCommentModel>>(userAccountOperationsHandler.GetUserAccount(email, user => user.LotComments).LotComments.Skip((page - 1) * amount).Take(amount)));
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
                return Ok(mapper.Map<LotCommentModel>(lotOperationsHandler.GetLot(lotId, lot => lot.LotComments).LotComments[commentNumber]));
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            value.UserId = User.Identity.Name;
            value.LotId = lotId;
            try
            {
                lotOperationsHandler.AddComment(mapper.Map<LotComment>(value));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}