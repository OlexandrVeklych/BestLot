﻿using System;
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
                cfg.CreateMap<LotModel, Lot>();
                cfg.CreateMap<Lot, LotModel>();
                cfg.CreateMap<LotCommentModel, LotComment>();
                cfg.CreateMap<LotComment, LotCommentModel>();
                cfg.CreateMap<LotPhotoModel, LotPhoto>();
                cfg.CreateMap<LotPhoto, LotPhotoModel>();
            }).CreateMapper();
            lotCommentsOperationsHandler = LogicDependencyResolver.ResolveLotCommentsOperationsHandler();
        }

        private readonly ILotCommentOperationsHandler lotCommentsOperationsHandler;
        private readonly IMapper mapper;
        // GET api/<controller>
        [Route("api/lots/{lotId}/comments")]
        public IHttpActionResult Get(int lotId, int page, int amount)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<LotCommentModel>>(lotCommentsOperationsHandler
                    .GetLotComments(lotId)
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
        public IHttpActionResult Get(string email, int page, int amount)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<LotCommentModel>>(lotCommentsOperationsHandler
                    .GetUserComments(email)
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
        public IHttpActionResult Get(int lotId, int commentNumber)
        {
            try
            {
                return Ok(mapper.Map<LotCommentModel>(lotCommentsOperationsHandler
                    .GetLotComments(lotId)
                    .ElementAt(commentNumber)));
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
        public IHttpActionResult Post([FromUri]int lotId, [FromBody]LotCommentModel value)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            value.UserId = User.Identity.Name;
            value.LotId = lotId;
            try
            {
                lotCommentsOperationsHandler.AddComment(mapper.Map<LotComment>(value));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}