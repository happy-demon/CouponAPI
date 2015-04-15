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

namespace MacysFetcher
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
            MacysFetcher();
        }

        public static void MacysFetcher()
        {
            var title = "";
            var link = "";
            var img = "";
            var rowKey = "";
            var originalPrice = "";
            var offerPrice = "";
            var des = "";

            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("Macys");

            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.
                Load("http://www1.macys.com/shop/makeup-and-perfume/gift-sets/Brand,Sortby,Productsperpage/Lanc%F4me,ORIGINAL,40?id=55537&edge=hybrid");

            var categories = htmlDocument.DocumentNode.
                SelectSingleNode("//ul[@class = 'thumbnails large-block-grid-4' and @id = 'thumbnails']");

            foreach (var product in categories.SelectNodes(".//div[@class = 'innerWrapper']"))
            {
                des = "";
                rowKey = Guid.NewGuid().ToString();
                link = "http://www1.macys.com" + product.SelectSingleNode(".//a[@href]").Attributes["href"].Value;

                try
                {
                    title = product.SelectSingleNode(".//img").Attributes["title"].Value;
                    img = product.SelectSingleNode(".//img").Attributes["data-src"].Value;
                }
                catch (Exception e)
                {
                    continue;
                }

                HtmlDocument htmlDocument2 = htmlWeb.Load(link);
                var originalPriceNode = htmlDocument2.DocumentNode.SelectSingleNode("//span[(@class = 'giftSetValue')]");
                var offerPriceNode = htmlDocument2.DocumentNode.SelectSingleNode("//span[(@class = 'priceSale')]");
                var standardPrice = htmlDocument2.DocumentNode.SelectSingleNode("//div[(@class = 'standardProdPricingGroup')]");

                if (originalPriceNode != null && offerPriceNode != null)
                {
                    originalPrice = originalPriceNode.InnerText.Split(' ')[1];
                    offerPrice = offerPriceNode.InnerText.Split(' ')[1];
                }
                else if (originalPriceNode != null && offerPriceNode == null)
                {
                    originalPrice = originalPriceNode.InnerText.Split(' ')[1];
                    offerPrice = "";

                }
                else if (originalPriceNode == null && offerPriceNode != null)
                {
                    originalPrice = "";
                    offerPrice = offerPriceNode.InnerText.Split(' ')[1];

                }
                else if (originalPriceNode == null && offerPriceNode == null && standardPrice != null)
                {
                    originalPrice = "";
                    var offerPrice_temp = standardPrice.InnerText.Replace("<!-- Below code is for member PDP's only -->", "");
                    offerPrice = offerPrice_temp.Replace("<!-- PRICE BLOCK: Single Price -->", "");
                }
                else
                {
                    originalPrice = "";
                    offerPrice = "";
                }

                HtmlDocument htmlDocument3 = htmlWeb.Load(link);
                var giftOffers = htmlDocument3.DocumentNode.SelectNodes("//div[(@class = 'giftOfferDetails')]");
                if (giftOffers == null)
                {
                    des = "";
                }
                else
                {
                    foreach (var x in giftOffers)
                    {
                        des += x.SelectSingleNode(".//div[(@class = 'giftOfferDescription')]").InnerText;
                    }
                }

                Console.WriteLine(title);
                Console.WriteLine(rowKey);
                Console.WriteLine(originalPrice);
                Console.WriteLine(offerPrice);
                Console.WriteLine(link);
                Console.WriteLine(img);
                Console.WriteLine(des);
                //Console.ReadLine();

                Macys product1 = new Macys("梅西商场", rowKey);
                product1.产品名称 = title;
                product1.产品分类 = "Beauty";
                product1.产品链接 = link;
                product1.产品图片 = img;
                product1.原价 = originalPrice;
                product1.折扣价 = offerPrice;
                product1.产品描述 = des;

                // Create the TableOperation that inserts the customer entity.
                TableOperation insertOperation = TableOperation.Insert(product1);

                // Execute the insert operation.
                table.Execute(insertOperation);
            }
        }

        public class Macys : TableEntity
        {
            public Macys(string category, string guid)
            {
                this.PartitionKey = category;
                this.RowKey = guid;
            }
            public Macys() { }
            public string 产品名称 { get; set; }
            public string 产品分类 { get; set; }
            public string 产品链接 { get; set; }
            public string 产品图片 { get; set; }
            public string 产品描述 { get; set; }
            public string 原价 { get; set; }
            public string 折扣价 { get; set; }
        }
    }
}
