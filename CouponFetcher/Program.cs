using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace CouponFetcher
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    public class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            FetchData();
        }

        private static void FetchData()
        {
            throw new NotImplementedException();
        }


        private static void CreateDemoData()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("input");
            container.CreateIfNotExists();

            CloudBlockBlob blob = container.GetBlockBlobReference("BlobOperations.txt");
            blob.UploadText("Hello world!");

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("persons");
            queue.CreateIfNotExists();

            Coupon c = new Coupon()
            {
                PartitionKey = "奥特莱",
                RowKey = "Abercrombie & 123" + DateTime.Now.ToFileTime(),
                Category = "test",
                CouponDetail = "CouponDetail",
                CouponEndDate= "01/10/2015", CouponImage ="http://test", CouponStartDate = "10/20/2015" , 
                ETag ="", OriginalPrice = "10.00", ProductDescription="ffff", ProductName="ffff", 
                SaleCity="seattle", SalePrice="8.20"
            };
            queue.AddMessage(new CloudQueueMessage(JsonConvert.SerializeObject(c)));
        }

    }

    public class Coupon : TableEntity
    {
        public string Category { get; set; }
        public string CouponDetail { get; set; }
        public string CouponEndDate { get; set; }
        public string CouponImage { get; set; }
        public string CouponStartDate { get; set; }
        public string OriginalPrice { get; set; }
        public string ProductDescription { get; set; }
        public string ProductName { get; set; }
        public string SaleCity { get; set; }
        public string SalePrice { get; set; }
    }

}
