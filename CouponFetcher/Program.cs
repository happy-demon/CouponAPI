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
                PartitionKey = "奥特莱",
                RowKey = "Abercrombie & 123" + DateTime.Now.ToFileTime(),
                Category = "test"//,
                //CouponDetail = "CouponDetail"//,
                //CouponEndDate= "01/10/2015", CouponImage ="http://test", CouponStartDate = "10/20/2015" , 
                //ETag ="", OriginalPrice = "10.00", ProductDescription="ffff", ProductName="ffff", 
                //SaleCity="seattle", SalePrice="8.20"
            };

            // Create the TableOperation that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(c);

            // Execute the insert operation.
            couponsTable.Execute(insertOperation);
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
