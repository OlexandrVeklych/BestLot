using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using BestLot.BusinessLogicLayer.Interfaces;
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
                cfg.CreateMap<LotCommentInModel, LotComment>();
                cfg.CreateMap<LotComment, LotCommentOutModel>();
            }).CreateMapper();
            lotCommentsOperationsHandler = LogicDependencyResolver.ResolveLotCommentOperationsHandler();
        }

        private readonly ILotCommentOperationsHandler lotCommentsOperationsHandler;
        private readonly IMapper mapper;

        // GET api/<controller>
        [Route("api/lots/{lotId}/comments")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetLotCommentsAsync(int lotId, int page, int amount)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<LotCommentOutModel>>((await lotCommentsOperationsHandler
                    .GetLotCommentsAsync(lotId))
                    .OrderBy(lotComment => lotComment.Id)
                    .Skip((page - 1) * amount)
                    .Take(amount).AsEnumerable()));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [Route("api/users/{email}/comments")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetUserCommentsAsync(string email, int page, int amount)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<LotCommentOutModel>>((await lotCommentsOperationsHandler
                    .GetUserCommentsAsync(email))
                    .OrderBy(lotComment => lotComment.Id)
                    .Skip((page - 1) * amount)
                    .Take(amount).AsEnumerable()));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<controller>/5
        [Route("api/lots/{lotId}/comments/{commentNumber}")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetLotCommentByNumberAsync(int lotId, int commentNumber)
        {
            try
            {
                return Ok(mapper.Map<LotCommentOutModel>((await lotCommentsOperationsHandler
                    .GetLotCommentsAsync(lotId))
                    .ToList()[commentNumber]));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (IndexOutOfRangeException)
            {
                return BadRequest("Wrong number of comment");
            }
        }

        [Authorize]
        // POST api/<controller>
        [Route("api/lots/{lotId}/comments")]
        public async System.Threading.Tasks.Task<IHttpActionResult> PostLotCommentAsync([FromUri]int lotId, [FromBody]LotCommentInModel value)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            value.UserId = User.Identity.Name;
            value.LotId = lotId;
            try
            {
                await lotCommentsOperationsHandler.AddCommentAsync(mapper.Map<LotComment>(value));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}