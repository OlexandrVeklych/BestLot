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
    public class LotPhotosController : ApiController
    {
        public LotPhotosController()
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
            lotPhotosOperationsHandler = LogicDependencyResolver.ResolveLotPhotosOperationsHandler();
        }

        private ILotPhotoOperationsHandler lotPhotosOperationsHandler;
        private readonly IMapper mapper;

        // GET api/<controller>
        [Route("api/lots/{lotId}/photos")]
        public IHttpActionResult Get(int lotId)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<LotPhotoModel>>(lotPhotosOperationsHandler.GetLotPhotos(lotId).AsEnumerable()));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<controller>/5
        [Route("api/lots/{lotId}/photos/{photoNumber}")]
        public IHttpActionResult Get(int lotId, int photoNumber)
        {
            try
            {
                return Ok(mapper.Map<LotPhotoModel>(lotPhotosOperationsHandler.GetLotPhotos(lotId).ElementAt(photoNumber)));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (IndexOutOfRangeException)
            {
                return BadRequest("Wrong number of photo");
            }
        }

        // POST api/<controller>
        [Route("api/lots/{lotId}/photos")]
        public IHttpActionResult Post([FromUri] int lotId, [FromBody]LotPhotoModel[] value)
        {
            try
            {
                lotPhotosOperationsHandler.AddPhotosToExistingLot(lotId, mapper.Map<LotPhoto[]>(value), System.Web.Hosting.HostingEnvironment.MapPath(@"~"), Request.RequestUri.GetLeftPart(UriPartial.Authority));
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
            try
            {
                lotPhotosOperationsHandler.DeletePhoto(id, System.Web.Hosting.HostingEnvironment.MapPath(@"~"), Request.RequestUri.GetLeftPart(UriPartial.Authority));
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}