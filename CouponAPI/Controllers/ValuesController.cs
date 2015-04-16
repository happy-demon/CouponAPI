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

    public class Coupons : TableEntity
    {
        public string ProductCategory { get; set; }
        public string ProductName { get; set; }
        public string ItemNumber { get; set; }
        public string ProductLink { get; set; }
        public string ProductPicture { get; set; }
        public string ProductDescription { get; set; }
        public string ProductDescription_cn { get; set; }
        public string OriginalPrice { get; set; }
        public string LessPrice { get; set; }
        public string OfferPrice { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ProductReview { get; set; }
    }
    public class Costco : TableEntity
    {
        public string 产品分类 { get; set; }
        public string 产品名称 { get; set; }
        public string 产品编号 { get; set; }
        public string 产品链接 { get; set; }
        public string 产品图片 { get; set; }
        public string 产品描述 { get; set; }
        public string 原价 { get; set; }
        public string 减价 { get; set; }
        public string 折扣价 { get; set; }
        public string 开始日期 { get; set; }
        public string 结束日期 { get; set; }
        public string 产品评价 { get; set; }
    }
    public class Macys : TableEntity
    {
        public string 产品名称 { get; set; }
        public string 产品分类 { get; set; }
        public string 产品链接 { get; set; }
        public string 产品图片 { get; set; }
        public string 产品描述 { get; set; }
        public string 原价 { get; set; }
        public string 折扣价 { get; set; }
    }
    public class Outlets : TableEntity
    {
        public string 商店名称 { get; set; }
        public string 开始日期 { get; set; }
        public string 结束日期 { get; set; }
        public string 活动描述 { get; set; }
    }

    public class CouponsController : ApiController
    {
        public static CloudTable table;
        // GET api/values
        public IEnumerable<Coupons> Get()
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable Coupons1 = tableClient.GetTableReference("Coupons1");
            CloudTable Coupons2 = tableClient.GetTableReference("Coupons2");

            if (Coupons1.Exists() == true)
            {
                table = tableClient.GetTableReference("Coupons1");
            }
            else if (Coupons2.Exists() == true)
            {
                table = tableClient.GetTableReference("Coupons2");
            }
            else
            {
                table = tableClient.GetTableReference("CouponsArchive");
            }

            var results = (from entity in table.CreateQuery<Coupons>()
                           //where entity.PartitionKey == "奥特莱"
                           select entity).Take(1000).ToList();

            return new List<Coupons>(results);

        }

        // GET api/values/5
        public Coupons Get(int id)
        {
            return null;
        }

        // POST api/values
        public void Post([FromBody]Coupons value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]Coupons value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }

    public class OutletsController : ApiController
    {

        // GET api/values
        public IEnumerable<Outlets> Get()
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("Outlets");

            var results = (from entity in table.CreateQuery<Outlets>()
                           //where entity.PartitionKey == "奥特莱"
                           select entity).Take(100).ToList();

            return new List<Outlets>(results);

        }

        // GET api/values/5
        public Outlets Get(int id)
        {
            return null;
        }

        // POST api/values
        public void Post([FromBody]Outlets value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]Outlets value)
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
        public IEnumerable<Costco> Get()
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("Costco");

            var results = (from entity in table.CreateQuery<Costco>()
                           //where entity.PartitionKey == "好市多"
                           select entity).Take(100).ToList();

            return new List<Costco>(results);

        }

        // GET api/values/5
        public Costco Get(int id)
        {
            return null;
        }

        // POST api/values
        public void Post([FromBody]Costco value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]Costco value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }

    public class MacysController : ApiController
    {

        // GET api/values
        public IEnumerable<Macys> Get()
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("Macys");

            var results = (from entity in table.CreateQuery<Macys>()
                           // where entity.PartitionKey == "奥特莱"
                           select entity).Take(100).ToList();

            return new List<Macys>(results);

        }

        // GET api/values/5
        public Macys Get(int id)
        {
            return null;
        }

        // POST api/values
        public void Post([FromBody]Macys value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]Macys value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
