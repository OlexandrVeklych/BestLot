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
    [Authorize]
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
                cfg.CreateMap<Func<LotModel, bool>, Func<Lot, bool>>();
            }).CreateMapper();
            lotOperationsHandler = LogicDependencyResolver.ResloveLotOperationsHandler();
            userAccountOperationsHandler = LogicDependencyResolver.ResloveUserAccountOperationsHandler();
        }

        private readonly ILotOperationsHandler lotOperationsHandler;
        private readonly IUserAccountOperationsHandler userAccountOperationsHandler;
        private readonly IMapper mapper;

        // GET api/<controller>
        [AllowAnonymous]
        public IHttpActionResult Get(int page, int amount)
        {
            return Ok(mapper.Map<IEnumerable<LotModel>>(lotOperationsHandler.GetAllLots(lot => lot.LotComments, lot => lot.LotPhotos, lot => lot.SellerUser).Skip((page - 1) * amount).Take(amount)));
        }

        [AllowAnonymous]
        [Route("api/users/{email}/lots")]
        public IHttpActionResult Get(string email, int page, int amount, string name = null, string category = null, double minPrice = 0, double maxPrice = 0)
        {
            Func<LotModel, bool> predicate = null;
            if (name != null)
                predicate += lot => lot.Name == name;
            if (category != null)
                predicate += lot => lot.Category == category;
            if (minPrice != 0)
                predicate += lot => lot.Price > minPrice;
            if (maxPrice != 0)
                predicate += lot => lot.Price < maxPrice;
            try
            {
                return Ok(mapper.Map<IEnumerable<LotModel>>(userAccountOperationsHandler.GetUserAccount(email, user => user.Lots).Lots.Where(mapper.Map<Func<Lot, bool>>(predicate)).Skip((page - 1) * amount).Take(amount)));
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        public IHttpActionResult Get(int page, int amount, string name = null, string category = null, double minPrice = 0, double maxPrice = 0)
        {
            Func<LotModel, bool> predicate = null;
            if (name != null)
                predicate += lot => lot.Name == name;
            if (category != null)
                predicate += lot => lot.Category == category;
            if (minPrice != 0)
                predicate += lot => lot.Price > minPrice;
            if (maxPrice != 0)
                predicate += lot => lot.Price < maxPrice;
            return Ok(mapper.Map<IEnumerable<LotModel>>(lotOperationsHandler.GetAllLots(lot => lot.LotComments, lot => lot.LotPhotos, lot => lot.SellerUser).Where(mapper.Map<Func<Lot, bool>>(predicate)).Skip((page - 1) * amount).Take(amount)));
        }
        // GET api/<controller>/5
        [AllowAnonymous]
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
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