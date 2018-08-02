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
using System.IO;
using System.Web;
using System.Drawing;
using AutoMapper.QueryableExtensions;
using System.Linq.Expressions;

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
        public IHttpActionResult Get(int page, int amount, string name = null, string category = null, double minPrice = 0, double maxPrice = 0)
        {
            IQueryable<Lot> result = lotOperationsHandler.GetAllLots(lot => lot.LotComments, lot => lot.LotPhotos, lot => lot.SellerUser);
            Expression<Func<Lot, bool>> predicate = null;
            if (name != null)
                result.Where(predicate = lot => lot.Name == name);
            if (category != null)
                result.Where(predicate = lot => lot.Category == category);
            if (minPrice != 0)
                result.Where(predicate = lot => lot.Price > minPrice);
            if (maxPrice != 0)
                result.Where(predicate = lot => lot.Price < maxPrice);
            return Ok(mapper.Map<IEnumerable<LotModel>>(result.OrderBy(lot => lot.Id).Skip((page - 1) * amount).Take(amount).AsEnumerable()));
        }
        //[AllowAnonymous]
        //public IHttpActionResult Get(int page, int amount)
        //{
        //    return Ok(lotOperationsHandler.GetAllLots(lot => lot.LotComments, lot => lot.LotPhotos, lot => lot.SellerUser).Skip((page - 1) * amount).Take(amount).ProjectTo<LotModel>(mapper.ConfigurationProvider));
        //}

        [AllowAnonymous]
        [Route("api/users/{email}/lots")]
        public IHttpActionResult Get(string email, int page, int amount, string name = null, string category = null, double minPrice = 0, double maxPrice = 0)
        {
            IQueryable<Lot> result = userAccountOperationsHandler.GetUserAccount(email, user => user.LotComments).Lots.AsQueryable();
            Expression<Func<Lot, bool>> predicate = null;
            if (name != null)
                result.Where(predicate = lot => lot.Name == name);
            if (category != null)
                result.Where(predicate = lot => lot.Category == category);
            if (minPrice != 0)
                result.Where(predicate = lot => lot.Price > minPrice);
            if (maxPrice != 0)
                result.Where(predicate = lot => lot.Price < maxPrice);
            try
            {
                return Ok(mapper.Map<IEnumerable<LotModel>>(result.OrderBy(lot => lot.Id).Skip((page - 1) * amount).Take(amount).AsEnumerable()));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
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
                SavePhotos(value);
                lotOperationsHandler.AddLot(mapper.Map<Lot>(value));
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }

        private void SavePhotos(LotModel lot)
        {
            string currentDirectory = System.Web.Hosting.HostingEnvironment.MapPath(@"~/Photos/") + lot.SellerUserId;
            if (!Directory.Exists(currentDirectory))
                Directory.CreateDirectory(currentDirectory);
            for (int i = 0; i < lot.LotPhotos.Count; i++)
            {
                if (lot.LotPhotos[i].Path.Count() < 1)
                    throw new Exception("Path count < 1");
                byte[] photoBytes = Convert.FromBase64String(lot.LotPhotos[i].Path);
                string photoPath = currentDirectory + "\\" + lot.Name + "_" + DateTime.Now.ToFileTime() + ".jpeg";
                File.WriteAllBytes(photoPath, photoBytes);
                lot.LotPhotos[i].Path = photoPath.Replace(currentDirectory, Request.RequestUri.GetLeftPart(UriPartial.Authority) + "\\Photos\\" + lot.SellerUserId);
            }
        


            //if (!Request.Content.IsMimeMultipartContent())
            //    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            //
            //var provider = new MultipartMemoryStreamProvider();
            //await Request.Content.ReadAsMultipartAsync(provider);
            //string currentDirectory = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data") + @"/LotPhotos/" + lot.SellerUserId;
            //if (!Directory.Exists(currentDirectory))
            //    Directory.CreateDirectory(currentDirectory);
            //for (int i = 0; i < provider.Contents.Count; i++)
            //{
            //    var buffer = await provider.Contents[0].ReadAsByteArrayAsync();
            //    Image photo;
            //
            //    using (var ms = new MemoryStream(buffer))
            //    {
            //        photo = Image.FromStream(ms);
            //    }
            //    string path = currentDirectory + @"/" + lot.Name + "_" + DateTime.Now.ToFileTime() + "_" + i + ".jpeg";
            //    photo.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
            //
            //    lot.LotPhotos[i].Path = path;
            //}

            //if (HttpContext.Current.Request.Files.Count == 0)
            //    throw new ArgumentException("No files");
            //string currentDirectory = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data") + @"/LotPhotos/" + lot.SellerUserId;
            //if (!Directory.Exists(currentDirectory))
            //    Directory.CreateDirectory(currentDirectory);
            //for (int i = 0; i < HttpContext.Current.Request.Files.Count; i++)
            //{
            //    byte[] fileData = null;
            //    using (var binaryReader = new BinaryReader(HttpContext.Current.Request.Files[0].InputStream))
            //    {
            //        fileData = binaryReader.ReadBytes(HttpContext.Current.Request.Files[0].ContentLength);
            //    }
            //
            //    Image photo;
            //
            //    using (var ms = new MemoryStream(fileData))
            //    {
            //        photo = Image.FromStream(ms);
            //    }
            //    string path = currentDirectory + "\\" + lot.Name + "_" + DateTime.Now.ToFileTime() + "_" + i + ".jpeg";
            //    photo.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
            //
            //    lot.LotPhotos[i].Path = path;
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