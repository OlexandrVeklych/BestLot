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
    public class UsersController : ApiController
    {
        public UsersController()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LotModel, Lot>();
                cfg.CreateMap<Lot, LotModel>();
                cfg.CreateMap<LotCommentModel, LotComment>();
                cfg.CreateMap<LotComment, LotCommentModel>();
                cfg.CreateMap<LotPhotoModel, LotPhoto>();
                cfg.CreateMap<LotPhoto, LotPhotoModel>();
                cfg.CreateMap<UserAccountInfo, UserAccountInfoModel>();
                cfg.CreateMap<UserAccountInfoModel, UserAccountInfo>();
            }).CreateMapper();
            userAccountOperationsHandler = LogicDependencyResolver.ResloveUserAccountOperationsHandler();
            lotOperationsHandler = LogicDependencyResolver.ResloveLotOperationsHandler();
        }

        private readonly IUserAccountOperationsHandler userAccountOperationsHandler;
        private readonly ILotOperationsHandler lotOperationsHandler;
        private readonly IMapper mapper;
        // GET api/<controller>
        public IHttpActionResult Get()
        {
            return Ok(mapper.Map<UserAccountInfoModel>(userAccountOperationsHandler.GetAllUserAccounts(user => user.LotComments, user => user.Lots)));
        }

        // GET api/<controller>/5
        [Route("api/lots/{id}/selleruser")]
        [Route("api/lots/{id}/buyeruser")]
        public IHttpActionResult Get(int id)
        {
            if (Request.RequestUri.OriginalString.Contains("buyeruser"))
                try
                {
                    return Ok(mapper.Map<UserAccountInfoModel>(userAccountOperationsHandler.GetUserAccount(lotOperationsHandler.GetLot(id).BuyerUserId)));
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }
            try
            {
                return Ok(mapper.Map<UserAccountInfoModel>(lotOperationsHandler.GetLot(id).SellerUser));
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
                return Ok(mapper.Map<UserAccountInfoModel>(userAccountOperationsHandler.GetUserAccount(email)));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<controller>
        [Route("api/users/{email}")]
        public IHttpActionResult Post([FromBody]string value)
        {
            return BadRequest("Use registration to add user");
        }

        // PUT api/<controller>/5
        [Route("api/users/{email}")]
        public IHttpActionResult Put(string email, [FromBody]UserAccountInfoModel value)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
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
    }
}