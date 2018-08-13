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
                cfg.CreateMap<UserAccountInfoModel, UserAccountInfo>();
                cfg.CreateMap<UserAccountInfo, UserAccountInfoModel>();
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
        public async System.Threading.Tasks.Task<IHttpActionResult> GetAllUsersAsync(int page, int amount)
        {
            return Ok(mapper.Map<IEnumerable<UserAccountInfoModel>>((await userAccountOperationsHandler
                .GetAllUserAccountsAsync())
                .OrderBy(user => user.Email)
                .Skip((page - 1) * amount)
                .Take(amount).AsEnumerable()));
        }

        [Route("api/currentuser")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetCurrentUserAsync()
        {
            try
            {
                return Ok(mapper.Map<UserAccountInfoModel>((await userAccountOperationsHandler
                    .GetUserAccountAsync(User.Identity.Name))));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<controller>/5
        [Route("api/lots/{lotId}/selleruser")]
        [Route("api/lots/{lotId}/buyeruser")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetLotRelatedUserAsync(int lotId)
        {
            if (Request.RequestUri.OriginalString.Contains("buyeruser"))
                try
                {
                    return Ok(mapper.Map<UserAccountInfoModel>((await userAccountOperationsHandler
                        .GetBuyerUserAsync(lotId))));
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }
            try
            {
                return Ok(mapper.Map<UserAccountInfoModel>((await userAccountOperationsHandler
                    .GetSellerUserAsync(lotId))));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<controller>/5
        [Route("api/users/{email}")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetUserAsync(string email)
        {
            try
            {
                return Ok(mapper.Map<UserAccountInfoModel>((await userAccountOperationsHandler
                    .GetUserAccountAsync(email))));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<controller>
        [Route("api/users")]
        public IHttpActionResult PostUser([FromBody]UserAccountInfoModel value)
        {
            return BadRequest("Use registration to add user");
        }

        // PUT api/<controller>/5
        [Route("api/users")]
        public async System.Threading.Tasks.Task<IHttpActionResult> PutUserAsync(string email, [FromBody]UserAccountInfoModel value)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (value.Email != User.Identity.Name || email != value.Email)
                return BadRequest("Not allowed");
            try
            {
                await userAccountOperationsHandler.ChangeUserAccountAsync(email, mapper.Map<UserAccountInfo>(value));
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}