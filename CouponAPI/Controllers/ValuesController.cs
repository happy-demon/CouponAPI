using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System.Configuration;

namespace CouponAPI.Controllers
{
    // [Authorize]

    public class Coupon : TableEntity
    {
        public string Category { get; set; }
        public string CouponDetail { get; set; }
        public DateTime CouponEndDate { get; set; }
        public string CouponImage { get; set; }
        public DateTime CouponStartDate { get; set; }
        public string OriginalPrice { get; set; }
        public string ProductDescription { get; set; }
        public string ProductName { get; set; }
        public string SaleCity { get; set; }
        public string SalePrice { get; set; }
    }

    public class OutletsController : ApiController
    {

        // GET api/values
        public IEnumerable<Coupon> Get()
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("Coupons");

            var results = (from Coupons in table.CreateQuery<Coupon>()
                           where Coupons.PartitionKey == "奥特莱"
                           select Coupons).Take(10).ToList();

            return new List<Coupon>(results);

        }

        // GET api/values/5
        public Coupon Get(int id)
        {
            return null;
        }

        // POST api/values
        public void Post([FromBody]Coupon value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]Coupon value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }

    public class CostcoController : ApiController
    {

        // GET api/values
        public IEnumerable<Coupon> Get()
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("Coupons");

            var results = (from Coupons in table.CreateQuery<Coupon>()
                           where Coupons.PartitionKey == "好市多"
                           select Coupons).Take(10).ToList();

            return new List<Coupon>(results);

        }

        // GET api/values/5
        public Coupon Get(int id)
        {
            return null;
        }

        // POST api/values
        public void Post([FromBody]Coupon value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]Coupon value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
