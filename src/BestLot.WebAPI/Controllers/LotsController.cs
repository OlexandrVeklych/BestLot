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
        }

        private readonly ILotOperationsHandler lotOperationsHandler;
        private readonly IMapper mapper;

        // GET api/<controller>
        [AllowAnonymous]
        public IHttpActionResult GetAllLots(int page, int amount, string name = null, string category = null, double minPrice = 0, double maxPrice = 0)
        {
            IQueryable<Lot> result = lotOperationsHandler.GetAllLots();
            Expression<Func<Lot, bool>> predicate = null;
            if (name != null && name != "null")
                result = result.Where(predicate = lot => lot.Name.Contains(name));
            if (category != null && category != "null")
                result = result.Where(predicate = lot => lot.Category.Contains(category));
            if (minPrice != 0)
                result = result.Where(predicate = lot => lot.Price > minPrice);
            if (maxPrice != 0)
                result = result.Where(predicate = lot => lot.Price < maxPrice);
            return Ok(mapper.Map<IEnumerable<LotOutModel>>(result
                .OrderBy(lot => lot.Id)
                .Skip((page - 1) * amount)
                .Take(amount).AsEnumerable()));
        }

        [AllowAnonymous]
        [Route("api/users/{email}/lots")]
        public IHttpActionResult GetUserLots(string email, int page, int amount, string name = null, string category = null, double minPrice = 0, double maxPrice = 0)
        {
            Expression<Func<Lot, bool>> predicate = null;
            IQueryable<Lot> result = lotOperationsHandler.GetUserLots(email);
            if (name != null && name != "null")
                result = result.Where(predicate = lot => lot.Name == name);
            if (category != null && category != "null")
                result = result.Where(predicate = lot => lot.Category == category);
            if (minPrice != 0)
                result = result.Where(predicate = lot => lot.Price > minPrice);
            if (maxPrice != 0)
                result = result.Where(predicate = lot => lot.Price < maxPrice);
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
        public IHttpActionResult GetLot(int id)
        {
            try
            {
                return Ok(mapper.Map<LotOutModel>(lotOperationsHandler.GetLot(id)));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("api/lots/{lotId}/bid")]
        public IHttpActionResult PostBid([FromUri]int lotId, [FromBody]double value)
        {
            if (!User.IsInRole("User"))
                return BadRequest("Sorry, admins and moderators can`t place bids");
            try
            {
                lotOperationsHandler.PlaceBid(lotId, User.Identity.Name, value);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
                // POST api/<controller>
        public IHttpActionResult PostLot([FromBody]LotInModel value)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            value.SellerUserId = User.Identity.Name;
            try
            {
                lotOperationsHandler.AddLot(mapper.Map<Lot>(value), System.Web.Hosting.HostingEnvironment.MapPath(@"~"), Request.RequestUri.GetLeftPart(UriPartial.Authority));
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }       

        // PUT api/<controller>/5
        public IHttpActionResult PutLot(int id, [FromBody]LotInModel value)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (value.SellerUserId != User.Identity.Name || id != value.Id)
                return BadRequest("Not allowed");
            try
            {
                lotOperationsHandler.ChangeLot(id, mapper.Map<Lot>(value), System.Web.Hosting.HostingEnvironment.MapPath(@"~"), Request.RequestUri.GetLeftPart(UriPartial.Authority));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }

        // DELETE api/<controller>/5
        public IHttpActionResult DeleteLot(int id)
        {
            if (lotOperationsHandler.GetLot(id).SellerUserId != User.Identity.Name && !User.IsInRole("Admin"))
                return BadRequest("Not Allowed");
            try
            {
                lotOperationsHandler.DeleteLot(id, System.Web.Hosting.HostingEnvironment.MapPath(@"~"), Request.RequestUri.GetLeftPart(UriPartial.Authority));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}