using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using System.IO;

namespace CouponFetcher
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    public class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            JobHost host = new JobHost();
            Task callTask = host.CallAsync(typeof(Program).GetMethod("ManualTrigger"));

            Console.WriteLine("Waiting for async operation...");
            callTask.Wait();
            Console.WriteLine("Task completed: " + callTask.Status);

        }


        [NoAutomaticTrigger]
        public static void ManualTrigger([Table("Coupons")] CloudTable couponsTable)
        {
            DateTime dt = DateTime.Now;

            Coupon c = new Coupon()
            {
                PartitionKey = dt.Year.ToString(),
                RowKey = DateTime.Now.ToString(),
                Category = "test",
                CouponDetail = "CouponDetail", CouponEndDate= DateTime.Now, CouponImage ="http://test", CouponStartDate = DateTime.Now , ETag ="", OriginalPrice = "10.00", ProductDescription="ffff", ProductName="ffff", SaleCity="seattle", SalePrice="8.20"
            };

            TableOperation operation = TableOperation.InsertOrReplace(c);

            couponsTable.Execute(operation);
        }
    }

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

}
