using BestLot.WebAPI.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BestLot.WebAPI.Controllers
{
    [Authorize(Roles ="Admin")]
    public class RolesController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get()
        {
            using (var context = new ApplicationDbContext())
            {
                var roleStore = new RoleStore<IdentityRole>(context);
                var roleManager = new RoleManager<IdentityRole>(roleStore);

                return Ok(roleManager.Roles);
            }
        }

        // GET api/<controller>/5
        [Route("api/roles/{roleName}")]
        public IHttpActionResult Get(string roleName)
        {
            using (var context = new ApplicationDbContext())
            {
                var roleStore = new RoleStore<IdentityRole>(context);
                var roleManager = new RoleManager<IdentityRole>(roleStore);

                return Ok(roleManager.FindByName(roleName));
            }
        }

        // POST api/<controller>
        public IHttpActionResult Post([FromBody]string roleName)
        {
            using (var context = new ApplicationDbContext())
            {
                var roleStore = new RoleStore<IdentityRole>(context);
                var roleManager = new RoleManager<IdentityRole>(roleStore);

                roleManager.Create(new IdentityRole() { Name = roleName });
                return Ok();
            }
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}