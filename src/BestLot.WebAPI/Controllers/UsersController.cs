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
        }

        private readonly IUserAccountOperationsHandler userAccountOperationsHandler;
        private readonly IMapper mapper;
        // GET api/<controller>
        public IHttpActionResult Get()
        {
            return Ok(mapper.Map<UserAccountInfoModel>(userAccountOperationsHandler.GetAllUserAccounts(user => user.LotComments, user => user.Lots)));
        }

        // GET api/<controller>/5
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
        public IHttpActionResult Post([FromBody]string value)
        {
            return BadRequest("User registration to add user");
        }

        // PUT api/<controller>/5
        public IHttpActionResult Put(string email, [FromBody]UserAccountInfoModel value)
        {
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
        public void Delete(int id)
        {
        }
    }
}