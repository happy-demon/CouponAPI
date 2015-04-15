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

namespace CostcoFetcher
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
            CostcoFetcher();
        }

        static void CostcoFetcher()
        {
            var categoryName = "";
            var title = "";
            var rowKey = "";
            var img = "";
            var des = "";
            var online_price = "";
            var less_price = "";
            var your_price = "";
            var startDate = "";
            var endDate = "";
            var review = "";

            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("Costco");

            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.Load("http://www.costco.com/vitamins-herbals-dietary-supplements.html");
            var category = htmlDocument.DocumentNode.SelectNodes("//div[@class = 'category-tile']");
            foreach (var product_col in category)
            {
                var col_link = product_col.SelectSingleNode(".//a[@href]").Attributes["href"].Value;
                categoryName = product_col.SelectSingleNode(".//a[@href]").Attributes["title"].Value.Replace("&amp;", "&");

                HtmlDocument htmlDocument1 = htmlWeb.Load(col_link);
                var products = htmlDocument1.DocumentNode.SelectNodes("//*[contains(@class,'product-tile-image-container')]");

                foreach (var product in products)
                {
                    rowKey = Guid.NewGuid().ToString();
                    var productInfo = product.SelectSingleNode(".//a[@href]");
                    var link = productInfo.Attributes["href"].Value;
                    var itemNumber = link.Split('.')[link.Split('.').Count() - 2];

                    foreach (var x in productInfo.SelectNodes(".//*[contains(@class,'short-desc')]"))
                    {
                        title = x.InnerText.Trim();
                    }

                    foreach (var x in productInfo.SelectNodes(".//img"))
                    {
                        img = x.Attributes["src"].Value;
                        des = x.Attributes["alt"].Value;
                    }

                    try
                    {
                        HtmlDocument htmlDocument2 = htmlWeb.Load(link);
                        var productDetail = htmlDocument2.DocumentNode.SelectNodes("//*[contains(@class,'product-price')]");
                        var productValid = htmlDocument2.DocumentNode.SelectNodes("//*[contains(@class,'col2')]");
                        foreach (var x in productValid)
                        {
                            var y = x.SelectSingleNode(".//*[contains(@class,'merchandisingText')]");
                            if (y.InnerText.Count() == 0)
                            {
                                startDate = "";
                                endDate = "";
                            }
                            else
                            {
                                var validDate = y.InnerText.Split(' ');
                                startDate = validDate[5];
                                endDate = validDate[7].Replace(".", string.Empty);
                            }
                        }
                        foreach (var price in productDetail)
                        {
                            var online_price_node = price.SelectSingleNode(".//*[contains(@class,'online-price')]");
                            online_price = online_price_node.SelectSingleNode(".//*[contains(@class,'currency')]").InnerText;
                            var less_price_node = price.SelectSingleNode(".//*[contains(@class,'less-price')]");
                            less_price = less_price_node.SelectSingleNode(".//*[contains(@class,'currency')]").InnerText;
                            var your_price_node = price.SelectSingleNode(".//*[contains(@class,'your-price')]");
                            your_price = your_price_node.SelectSingleNode(".//*[contains(@class,'currency')]").InnerText;
                        }
                    }
                    catch (Exception e)
                    {
                        continue;
                    }


                    var productReview = product.SelectSingleNode(".//*[contains(@class,'product-rating')]");
                    if (productReview == null)
                    {
                        review = "";
                    }
                    else
                    {
                        review = productReview.InnerText.Substring(0, 3) + "星(共5星)";
                    }

                    Console.WriteLine(title);
                    Console.WriteLine(itemNumber);
                    Console.WriteLine(link);
                    Console.WriteLine(img);
                    Console.WriteLine(des);
                    Console.WriteLine(online_price);
                    Console.WriteLine(less_price);
                    Console.WriteLine(your_price);
                    Console.WriteLine(startDate);
                    Console.WriteLine(endDate);
                    Console.WriteLine(review);
                    Console.WriteLine(rowKey);
                    //Console.ReadLine();

                    // Create a new customer entity.
                    Costco product1 = new Costco("好市多", rowKey);
                    product1.产品分类 = categoryName;
                    product1.产品名称 = title;
                    product1.产品编号 = itemNumber;
                    product1.产品链接 = link;
                    product1.产品图片 = img;
                    product1.产品描述 = des;
                    product1.原价 = online_price;
                    product1.减价 = less_price;
                    product1.折扣价 = your_price;
                    product1.开始日期 = startDate;
                    product1.结束日期 = endDate;
                    product1.产品评价 = review;

                    // Create the TableOperation that inserts the customer entity.
                    TableOperation insertOperation = TableOperation.Insert(product1);

                    // Execute the insert operation.
                    table.Execute(insertOperation);
                }
            }
        }

        public class Costco : TableEntity
        {
            public Costco(string category, string guid)
            {
                this.PartitionKey = category;
                this.RowKey = guid;
            }
            public Costco() { }
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
    }
}
