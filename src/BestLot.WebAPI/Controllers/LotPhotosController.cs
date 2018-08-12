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

                cfg.CreateMap<LotPhotoInModel, LotPhoto>();
                cfg.CreateMap<LotPhoto, LotPhotoOutModel>();
            }).CreateMapper();
            lotPhotosOperationsHandler = LogicDependencyResolver.ResolveLotPhotoOperationsHandler();
        }

        private ILotPhotoOperationsHandler lotPhotosOperationsHandler;
        private readonly IMapper mapper;

        // GET api/<controller>
        [Route("api/lots/{lotId}/photos")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetLotPhotosAsync(int lotId)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<LotPhotoOutModel>>((await lotPhotosOperationsHandler
                    .GetLotPhotosAsync(lotId))
                    .AsEnumerable()));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<controller>/5
        [Route("api/lots/{lotId}/photos/{photoNumber}")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetLotPhotoByNumberAsync(int lotId, int photoNumber)
        {
            try
            {
                return Ok(mapper.Map<LotPhotoOutModel>(await lotPhotosOperationsHandler
                    .GetLotPhotoByNumberAsync(lotId, photoNumber)));
            }
            catch (IndexOutOfRangeException)
            {
                return BadRequest("Wrong number of photo");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<controller>
        [Route("api/lots/{lotId}/photos")]
        public async System.Threading.Tasks.Task<IHttpActionResult> PostLotPhotoAsync([FromUri] int lotId, [FromBody]LotPhotoInModel[] value)
        {
            try
            {
                await lotPhotosOperationsHandler.AddPhotosToExistingLotAsync(lotId, mapper.Map<LotPhoto[]>(value), System.Web.Hosting.HostingEnvironment.MapPath(@"~"), Request.RequestUri.GetLeftPart(UriPartial.Authority));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }

        // DELETE api/<controller>/5
        public async System.Threading.Tasks.Task<IHttpActionResult> DeleteLotPhotoAsync(int id)
        {
            try
            {
                await lotPhotosOperationsHandler.DeletePhotoAsync(id, System.Web.Hosting.HostingEnvironment.MapPath(@"~"), Request.RequestUri.GetLeftPart(UriPartial.Authority));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}