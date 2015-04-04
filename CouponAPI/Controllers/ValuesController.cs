using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CouponAPI.Controllers
{
    // [Authorize]
    public class Store
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class StoreController : ApiController
    {
        // GET api/values
        public IEnumerable<Store> Get()
        {
            return new List<Store> {
                new Store{ ID = 0, Name = "Costco" },
                new Store{ ID = 1, Name = "Macy" },
                new Store{ ID = 2, Name = "Outlets" }
            };
        }

        // GET api/values/5
        public Store Get(int id)
        {
            return null;
        }

        // POST api/values
        public void Post([FromBody]Store value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]Store value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
