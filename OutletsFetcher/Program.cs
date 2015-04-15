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
using HtmlAgilityPack;

namespace OutletsFetcher
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var host = new JobHost();
            // The following code ensures that the WebJob will be running continuously
            OutletsFetcher();
        }

        public static void OutletsFetcher()
        {
            var storeName = "";
            var startDate = "";
            var rowKey = "";
            var endDate = "";
            var eventDescription = "";

            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("Outlets");

            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.Load("http://www.premiumoutlets.com/outlets/sales.asp?id=71");

            string html = htmlDocument.DocumentNode.OuterHtml;

            IEnumerable<HtmlNode> listItems = htmlDocument.DocumentNode.SelectNodes("//*[contains(@class,'StoreEvents')]");

            var storeEvents = "";
            foreach (var item in listItems)
            {
                storeEvents = item.InnerHtml;
            }

            string splitStr = "<div style='border-top: 1px solid #E2E2E2; margin:8px 0 6px 0;padding:0;position:relative;'></div>";
            string[] colCoupons = storeEvents.Split(new string[] { splitStr }, StringSplitOptions.None);

            foreach (var x in colCoupons)
            {
                rowKey = Guid.NewGuid().ToString();
                var coupon = x.Replace("<h4 class='cap mb-10'>", "").Replace("</h4>", "").Replace("<br>", "");
                storeName = coupon.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];
                var eventDate = coupon.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[1];
                var eventTemp = coupon.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                List<string> eventDescription_li = new List<string>(eventTemp);
                eventDescription_li.RemoveRange(0, 2);
                eventDescription = string.Join("\n", eventDescription_li.ToArray());

                if (eventDate.Split(' ').Count() == 5)
                {
                    startDate = (DateTime.Parse(eventDate.Split(' ')[0] + " "
                        + eventDate.Split(' ')[1] + " " + eventDate.Split(' ')[4])).ToString().Split(' ')[0];
                    endDate = (DateTime.Parse(eventDate.Split(' ')[0] + " "
                        + eventDate.Split(' ')[3] + " " + eventDate.Split(' ')[4])).ToString().Split(' ')[0];
                }
                else if (eventDate.Split(' ').Count() == 6)
                {
                    startDate = (DateTime.Parse(eventDate.Split(' ')[0] + " "
                        + eventDate.Split(' ')[1] + " " + eventDate.Split(' ')[5])).ToString().Split(' ')[0];
                    endDate = (DateTime.Parse(eventDate.Split(' ')[3] + " "
                        + eventDate.Split(' ')[4] + " " + eventDate.Split(' ')[5])).ToString().Split(' ')[0];
                }
                else
                {
                    startDate = (DateTime.Parse(eventDate.Split(' ')[0] + " "
                        + eventDate.Split(' ')[1] + " " + eventDate.Split(' ')[6])).ToString().Split(' ')[0];
                    endDate = (DateTime.Parse(eventDate.Split(' ')[4] + " "
                        + eventDate.Split(' ')[5] + " " + eventDate.Split(' ')[6])).ToString().Split(' ')[0];
                }

                Console.WriteLine(storeName);
                Console.WriteLine(startDate);
                Console.WriteLine(endDate);
                Console.WriteLine(eventDescription);
                //Console.ReadLine();

                // Create a new customer entity.
                Outlets product1 = new Outlets("奥特莱斯", rowKey);
                product1.商店名称 = storeName;
                product1.开始日期 = startDate;
                product1.结束日期 = endDate;
                product1.活动描述 = eventDescription;

                // Create the TableOperation that inserts the customer entity.
                TableOperation insertOperation = TableOperation.Insert(product1);

                // Execute the insert operation.
                table.Execute(insertOperation);
            }
        }

        public class Outlets : TableEntity
        {
            public Outlets(string category, string guid)
            {
                this.PartitionKey = category;
                this.RowKey = guid;
            }
            public Outlets() { }
            public string 商店名称 { get; set; }
            public string 开始日期 { get; set; }
            public string 结束日期 { get; set; }
            public string 活动描述 { get; set; }
        }
    }
}
