using AutoMapper;
using BestLot.BusinessLogicLayer;
using BestLot.BusinessLogicLayer.Exceptions;
using BestLot.BusinessLogicLayer.Interfaces;
using BestLot.BusinessLogicLayer.Models;
using BestLot.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BestLot.WebAPI.Controllers
{
    [Authorize]
    public class LotArchiveController : ApiController
    {
        public LotArchiveController()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Lot, LotModel>();
            }).CreateMapper();
            lotArchiveOperationsHandler = LogicDependencyResolver.ResolveLotArchiveOperationsHandler();
        }

        private readonly ILotArchiveOperationsHandler lotArchiveOperationsHandler;
        private readonly IMapper mapper;

        // GET
        [Route("api/users/{email}/archivedlots")]
        public async System.Threading.Tasks.Task<IHttpActionResult> GetUserLotsAsync(string email, int page, int amount)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<LotModel>>((await lotArchiveOperationsHandler.GetUserArchivedLotsAsync(email))
                    .OrderBy(lot => lot.Id)
                    .Skip((page - 1) * amount)
                    .Take(amount).AsEnumerable()));
            }
            catch (WrongIdException ex)
            {
                return Content(HttpStatusCode.NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (lotArchiveOperationsHandler != null)
                {
                    lotArchiveOperationsHandler.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}