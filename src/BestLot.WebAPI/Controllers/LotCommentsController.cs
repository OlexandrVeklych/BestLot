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
using BestLot.BusinessLogicLayer.Exceptions;

namespace BestLot.WebAPI.Controllers
{
    public class LotCommentsController : ApiController
    {
        public LotCommentsController()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LotCommentModel, LotComment>();
                cfg.CreateMap<LotComment, LotCommentModel>();
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
                return Ok(mapper.Map<IEnumerable<LotCommentModel>>((await lotCommentsOperationsHandler
                    .GetLotCommentsAsync(lotId))
                    .OrderBy(lotComment => lotComment.Id)
                    .Skip((page - 1) * amount)
                    .Take(amount).AsEnumerable()));
            }
            catch (WrongIdException ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
            catch (WrongModelException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Authorize(Roles = "Admin")]
        [Route("api/users/{email}/comments")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetUserCommentsAsync(string email, int page, int amount)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<LotCommentModel>>((await lotCommentsOperationsHandler
                    .GetUserCommentsAsync(email))
                    .OrderBy(lotComment => lotComment.Id)
                    .Skip((page - 1) * amount)
                    .Take(amount).AsEnumerable()));
            }
            catch (WrongIdException ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
            catch (WrongModelException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/<controller>/5
        [Route("api/lots/{lotId}/comments/{commentNumber}")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetLotCommentByNumberAsync(int lotId, int commentNumber)
        {
            try
            {
                return Ok(mapper.Map<LotCommentModel>((await lotCommentsOperationsHandler
                    .GetLotCommentsAsync(lotId))
                    .ToList()[commentNumber]));
            }
            catch (WrongIdException ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
            catch (WrongModelException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Authorize]
        // POST api/<controller>
        [Route("api/lots/{lotId}/comments")]
        public async System.Threading.Tasks.Task<IHttpActionResult> PostLotCommentAsync([FromUri]int lotId, [FromBody]LotCommentModel value)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            value.UserId = User.Identity.Name;
            value.LotId = lotId;
            try
            {
                await lotCommentsOperationsHandler.AddCommentAsync(mapper.Map<LotComment>(value));
            }
            catch (WrongIdException ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
            catch (WrongModelException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (lotCommentsOperationsHandler != null)
                {
                    lotCommentsOperationsHandler.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}