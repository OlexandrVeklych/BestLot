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

namespace BestLot.WebAPI.Controllers
{
    public class UsersController : ApiController
    {
        public UsersController()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserAccountInfoInModel, UserAccountInfo>();
                cfg.CreateMap<UserAccountInfo, UserAccountInfoOutModel>();
            }).CreateMapper();
            userAccountOperationsHandler = LogicDependencyResolver.ResolveUserAccountOperationsHandler();
            lotOperationsHandler = LogicDependencyResolver.ResolveLotOperationsHandler();
        }

        private readonly IUserAccountOperationsHandler userAccountOperationsHandler;
        private readonly ILotOperationsHandler lotOperationsHandler;
        private readonly IMapper mapper;

        [Authorize(Roles = "Admin")]
        // GET api/<controller>
        [Route("api/users")]
        public IHttpActionResult Get(int page, int amount)
        {
            return Ok(mapper.Map<IEnumerable<UserAccountInfoOutModel>>(userAccountOperationsHandler
                .GetAllUserAccounts()
                .OrderBy(user => user.Email)
                .Skip((page - 1) * amount)
                .Take(amount).AsEnumerable()));
        }

        [Route("api/currentuser")]
        public IHttpActionResult GetCurrentUser()
        {
            try
            {
                return Ok(mapper.Map<UserAccountInfoOutModel>(userAccountOperationsHandler
                    .GetUserAccount(User.Identity.Name)));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<controller>/5
        [Route("api/lots/{lotId}/selleruser")]
        [Route("api/lots/{lotId}/buyeruser")]
        public IHttpActionResult Get(int lotId)
        {
            if (Request.RequestUri.OriginalString.Contains("buyeruser"))
                try
                {
                    return Ok(mapper.Map<UserAccountInfoOutModel>(userAccountOperationsHandler
                        .GetBuyerUser(lotId)));
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }
            try
            {
                return Ok(mapper.Map<UserAccountInfoOutModel>(userAccountOperationsHandler
                    .GetSellerUser(lotId)));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<controller>/5
        [Route("api/users/{email}")]
        public IHttpActionResult Get(string email)
        {
            try
            {
                return Ok(mapper.Map<UserAccountInfoOutModel>(userAccountOperationsHandler.GetUserAccount(email)));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<controller>
        [Route("api/users")]
        public IHttpActionResult Post([FromBody]UserAccountInfoInModel value)
        {
            return BadRequest("Use registration to add user");
        }

        // PUT api/<controller>/5
        [Route("api/users/{email}")]
        public IHttpActionResult Put([FromUri]string email, [FromBody]UserAccountInfoInModel value)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (value.Email != User.Identity.Name || email != value.Email)
                return BadRequest("Not allowed");
            try
            {
                userAccountOperationsHandler.ChangeUserAccount(email, mapper.Map<UserAccountInfo>(value));
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/<controller>/5
        [Route("api/users/{email}")]
        public IHttpActionResult Delete(string email)
        {
            if (!User.IsInRole("Admin") && User.Identity.Name != email)
                return BadRequest("Not allowed");
            try
            {
                userAccountOperationsHandler.DeleteUserAccount(email, System.Web.Hosting.HostingEnvironment.MapPath(@"~"));
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}