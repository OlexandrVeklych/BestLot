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
using BestLot.BusinessLogicLayer.Exceptions;

namespace BestLot.WebAPI.Controllers
{
    public class LotPhotosController : ApiController
    {
        public LotPhotosController()
        {
            mapper = new MapperConfiguration(cfg =>
            {

                cfg.CreateMap<LotPhotoModel, LotPhoto>();
                cfg.CreateMap<LotPhoto, LotPhotoModel>();
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
                return Ok(mapper.Map<IEnumerable<LotPhotoModel>>((await lotPhotosOperationsHandler
                    .GetLotPhotosAsync(lotId))
                    .AsEnumerable()));
            }
            catch (WrongIdException ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
            catch (WrongModelException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/<controller>/5
        [Route("api/lots/{lotId}/photos/{photoNumber}")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetLotPhotoByNumberAsync(int lotId, int photoNumber)
        {
            try
            {
                return Ok(mapper.Map<LotPhotoModel>(await lotPhotosOperationsHandler
                    .GetLotPhotoByNumberAsync(lotId, photoNumber)));
            }
            catch (WrongIdException ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
            catch (WrongModelException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST api/<controller>
        [Route("api/lots/{lotId}/photos")]
        public async System.Threading.Tasks.Task<IHttpActionResult> PostLotPhotoAsync([FromUri] int lotId, [FromBody]LotPhotoModel[] value)
        {
            try
            {
                await lotPhotosOperationsHandler.AddPhotosToExistingLotAsync(lotId, mapper.Map<LotPhoto[]>(value), System.Web.Hosting.HostingEnvironment.MapPath(@"~"), Request.RequestUri.GetLeftPart(UriPartial.Authority));
            }
            catch (WrongIdException ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
            catch (WrongModelException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
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
            catch (WrongIdException ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
            catch (WrongModelException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (lotPhotosOperationsHandler != null)
                {
                    lotPhotosOperationsHandler.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}