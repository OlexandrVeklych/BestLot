using System;
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

namespace BestLot.WebAPI.Controllers
{
    [Authorize]
    public class LotsController : ApiController
    {
        public LotsController()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LotInModel, Lot>();
                cfg.CreateMap<Lot, LotOutModel>();
                cfg.CreateMap<LotPhotoInModel, LotPhoto>();
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
                return Ok(mapper.Map<IEnumerable<LotOutModel>>(result
                    .OrderBy(lot => lot.Id)
                    .Skip((page - 1) * amount)
                    .Take(amount).AsEnumerable()));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

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
                return Ok(mapper.Map<IEnumerable<LotOutModel>>(result
                    .OrderBy(lot => lot.Id)
                    .Skip((page - 1) * amount)
                    .Take(amount).AsEnumerable()));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<controller>/5
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetLotAsync(int id)
        {
            try
            {
                return Ok(mapper.Map<LotOutModel>(await lotOperationsHandler.GetLotAsync(id)));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("api/lots/{lotId}/bid")]
        public async System.Threading.Tasks.Task<IHttpActionResult> PostBidAsync([FromUri]int lotId, [FromBody]double value)
        {
            if (!User.IsInRole("User"))
                return BadRequest("Sorry, admins and moderators can`t place bids");
            try
            {
                await lotOperationsHandler.PlaceBidAsync(lotId, User.Identity.Name, value);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [Route("api/lots/{lotId}/bid")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetBidInfoAsync([FromUri]int lotId)
        {
            try
            {
                LotOutModel lot =  mapper.Map<LotOutModel>(await lotOperationsHandler.GetLotAsync(lotId));
                UserAccountInfoOutModel buyerUser = null;
                if (lot.BuyerUserId != null)
                    buyerUser = mapper.Map<UserAccountInfoOutModel>(await userAccountOperationsHandler.GetUserAccountAsync(lot.BuyerUserId));
                if (lot.BidPlacer == 1)
                    return Ok(new { lot.Price, BuyerUser = buyerUser });
                return Ok(new { lot.Price, lot.StartDate, lot.SellDate, BuyerUser = buyerUser });
                //return Ok(new {
                //    Price = lotOperationsHandler.GetLotPrice(lotId),
                //    StartDate = lotOperationsHandler.GetLotStartDate(lotId),
                //    SellDate = lotOperationsHandler.GetLotSellDate(lotId),
                //    BuyerUser = userAccountOperationsHandler.GetBuyerUser(lotId)
                //});
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<controller>
        public async System.Threading.Tasks.Task<IHttpActionResult> PostLotAsync([FromBody]LotInModel value)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            value.SellerUserId = User.Identity.Name;
            if (value.BidPlacer == 2)
            {
                value.SellDate = value.StartDate.Add(value.SellDate.Subtract(value.StartDate));
            }
            try
            {
                await lotOperationsHandler.AddLotAsync(mapper.Map<Lot>(value), System.Web.Hosting.HostingEnvironment.MapPath(@"~"), Request.RequestUri.GetLeftPart(UriPartial.Authority));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }

        // PUT api/<controller>/5
        public async System.Threading.Tasks.Task<IHttpActionResult> PutLotAsync(int id, [FromBody]LotInModel value)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (value.SellerUserId != User.Identity.Name || id != value.Id)
                return BadRequest("Not allowed");
            try
            {
                await lotOperationsHandler.ChangeLotAsync(id, mapper.Map<Lot>(value), System.Web.Hosting.HostingEnvironment.MapPath(@"~"), Request.RequestUri.GetLeftPart(UriPartial.Authority));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }

        // DELETE api/<controller>/5
        public async System.Threading.Tasks.Task<IHttpActionResult> DeleteLotAsync(int id)
        {
            if (lotOperationsHandler.GetLot(id).SellerUserId != User.Identity.Name && !User.IsInRole("Admin"))
                return BadRequest("Not Allowed");
            try
            {
                await lotOperationsHandler.DeleteLotAsync(id, System.Web.Hosting.HostingEnvironment.MapPath(@"~"), Request.RequestUri.GetLeftPart(UriPartial.Authority));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}