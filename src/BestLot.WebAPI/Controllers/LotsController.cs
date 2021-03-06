﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BestLot.BusinessLogicLayer.Interfaces;
using BestLot.BusinessLogicLayer.Models;
using BestLot.BusinessLogicLayer;
using BestLot.WebAPI.Models;
using AutoMapper;
using System.Linq.Expressions;
using BestLot.BusinessLogicLayer.Exceptions;

namespace BestLot.WebAPI.Controllers
{
    [Authorize]
    public class LotsController : ApiController
    {
        public LotsController()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LotModel, Lot>();
                cfg.CreateMap<Lot, LotModel>();
                cfg.CreateMap<LotPhotoModel, LotPhoto>();
            }).CreateMapper();
            lotOperationsHandler = LogicDependencyResolver.ResolveLotOperationsHandler();
            userAccountOperationsHandler = LogicDependencyResolver.ResolveUserAccountOperationsHandler();
        }

        private readonly IUserAccountOperationsHandler userAccountOperationsHandler;
        private readonly ILotOperationsHandler lotOperationsHandler;
        private readonly IMapper mapper;

        // GET api/<controller>
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetAllLotsAsync(int page, int amount, string name = null, string category = null, double minPrice = 0, double maxPrice = 0)
        {
            IQueryable<Lot> result = await lotOperationsHandler.GetAllLotsAsync();
            Expression<Func<Lot, bool>> predicate = null;
            if (name != null && name != "null")
                result = result.Where(predicate = lot => lot.Name.Contains(name));
            if (category != null && category != "null")
                result = result.Where(predicate = lot => lot.Category.Contains(category));
            if (minPrice != 0)
                result = result.Where(predicate = lot => lot.Price >= minPrice);
            if (maxPrice != 0)
                result = result.Where(predicate = lot => lot.Price <= maxPrice);
            try
            {
                return Ok(mapper.Map<IEnumerable<LotModel>>(result
                    .OrderBy(lot => lot.Id)
                    .Skip((page - 1) * amount)
                    .Take(amount).AsEnumerable()));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET
        [AllowAnonymous]
        [Route("api/users/{email}/lots")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetUserLotsAsync(string email, int page, int amount, string name = null, string category = null, double minPrice = 0, double maxPrice = 0)
        {
            IQueryable<Lot> result = await lotOperationsHandler.GetUserLotsAsync(email);
            Expression<Func<Lot, bool>> predicate = null;
            if (name != null && name != "null")
                result = result.Where(predicate = lot => lot.Name.Contains(name));
            if (category != null && category != "null")
                result = result.Where(predicate = lot => lot.Category.Contains(category));
            if (minPrice != 0)
                result = result.Where(predicate = lot => lot.Price >= minPrice);
            if (maxPrice != 0)
                result = result.Where(predicate = lot => lot.Price <= maxPrice);

            try
            {
                return Ok(mapper.Map<IEnumerable<LotModel>>(result
                    .OrderBy(lot => lot.Id)
                    .Skip((page - 1) * amount)
                    .Take(amount).AsEnumerable()));
            }
            catch (WrongIdException ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/<controller>/5
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetLotAsync(int id)
        {
            try
            {
                return Ok(mapper.Map<LotModel>(await lotOperationsHandler.GetLotAsync(id)));
            }
            catch (WrongIdException ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST
        [Authorize]
        [Route("api/lots/{lotId}/bid")]
        public async System.Threading.Tasks.Task<IHttpActionResult> PostBidAsync([FromUri]int lotId, [FromBody]double value)
        {
            try
            {
                await lotOperationsHandler.PlaceBidAsync(lotId, User.Identity.Name, value);
                return Ok();
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

        // GET
        [AllowAnonymous]
        [Route("api/lots/{lotId}/bid")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetBidInfoAsync([FromUri]int lotId)
        {
            try
            {
                var (Price, StartDate, SellDate) = await lotOperationsHandler.GetBidInfoAsync(lotId);
                UserAccountInfoModel BuyerUser = mapper.Map<UserAccountInfoModel>(await userAccountOperationsHandler.GetBuyerUserAsync(lotId));
                return Ok(new { Price, StartDate, SellDate, BuyerUser });
            }
            catch (WrongIdException ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST api/<controller>
        public async System.Threading.Tasks.Task<IHttpActionResult> PostLotAsync([FromBody]LotModel value)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            value.SellerUserId = User.Identity.Name;
            try
            {
                await lotOperationsHandler.AddLotAsync(mapper.Map<Lot>(value), System.Web.Hosting.HostingEnvironment.MapPath(@"~"), Request.RequestUri.GetLeftPart(UriPartial.Authority));
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

        // PUT api/<controller>/{id}
        public async System.Threading.Tasks.Task<IHttpActionResult> PutLotAsync(int id, [FromBody]LotModel value)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (value.SellerUserId != User.Identity.Name || id != value.Id)
                return BadRequest("Not allowed");
            try
            {
                await lotOperationsHandler.ChangeLotAsync(id, mapper.Map<Lot>(value));
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

        // DELETE api/<controller>/{id}
        public async System.Threading.Tasks.Task<IHttpActionResult> DeleteLotAsync(int id)
        {
            if (lotOperationsHandler.GetLot(id).SellerUserId != User.Identity.Name && !User.IsInRole("Admin"))
                return BadRequest("Not allowed");
            try
            {
                await lotOperationsHandler.DeleteLotAsync(id, System.Web.Hosting.HostingEnvironment.MapPath(@"~"), Request.RequestUri.GetLeftPart(UriPartial.Authority));
            }
            catch (WrongIdException ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("api/RefreshDBs")]
        public IHttpActionResult RefreshDBs()
        {
            lotOperationsHandler.RefreshDBs();
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (lotOperationsHandler != null)
                {
                    lotOperationsHandler.Dispose();
                }
                if (userAccountOperationsHandler != null)
                {
                    userAccountOperationsHandler.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}