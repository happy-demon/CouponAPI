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

namespace Fetcher
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
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

        public class Outlets : TableEntity
        {
            public Outlets(string category, string guid)
            {
                this.PartitionKey = category;
                this.RowKey = guid;
            }
            public Outlets() { }
            public string 产品名称 { get; set; }
            public string 开始日期 { get; set; }
            public string 结束日期 { get; set; }
            public string 产品描述 { get; set; }
        }

        public static CloudTable table;
        public static CloudTable couponsArchive;

        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var host = new JobHost();
            // The following code ensures that the WebJob will be running continuously
            //host.RunAndBlock();

            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable Coupons1 = tableClient.GetTableReference("Coupons1");
            CloudTable Coupons2 = tableClient.GetTableReference("Coupons2");
            couponsArchive = tableClient.GetTableReference("CouponsArchive");

            if (Coupons1.Exists() == true && Coupons2.Exists() == false)
            {
                Coupons1.DeleteIfExists();
                table = tableClient.GetTableReference("Coupons2");
                table.CreateIfNotExists();
            }
            else if (Coupons1.Exists() == false && Coupons2.Exists() == true)
            {
                Coupons2.DeleteIfExists();
                table = tableClient.GetTableReference("Coupons1");
                table.CreateIfNotExists();
            }
            else
            {
                table = tableClient.GetTableReference("Coupons1");
                table.CreateIfNotExists();
            }

            CostcoFetcher();
            MacysFetcher();
            OutletsFetcher();
        }

        public static void CostcoFetcher()
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
            var costcoLink = "http://www.costco.com/vitamins-herbals-dietary-supplements.html";

            // Retrieve the storage account from the connection string.
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            //    ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            // Create the table client.
            //CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            //CloudTable table = tableClient.GetTableReference("Coupons");

            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.Load(costcoLink);
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
                    try
                    {
                        table.Execute(insertOperation);
                        couponsArchive.Execute(insertOperation);
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
            }
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
            var macysLink = "http://www1.macys.com/shop/makeup-and-perfume/gift-sets/Brand,Sortby,Productsperpage/Lanc%F4me,ORIGINAL,40?id=55537&edge=hybrid";

            // Retrieve the storage account from the connection string.
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            //    ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            // Create the table client.
            //CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            //CloudTable table = tableClient.GetTableReference("Coupons");

            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.
                Load(macysLink);

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
                product1.原价 = originalPrice.Trim();
                product1.折扣价 = offerPrice.Trim();
                product1.产品描述 = des;

                // Create the TableOperation that inserts the customer entity.
                TableOperation insertOperation = TableOperation.Insert(product1);

                // Execute the insert operation.
                try
                {
                    table.Execute(insertOperation);
                    couponsArchive.Execute(insertOperation);
                }
                catch (Exception e)
                {
                    continue;
                }
            }
        }

        public static void OutletsFetcher()
        {
            var storeName = "";
            var startDate = "";
            var rowKey = "";
            var endDate = "";
            var eventDescription = "";
            var outletsLink = "http://www.premiumoutlets.com/outlets/sales.asp?id=71";

            // Retrieve the storage account from the connection string.
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            //    ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            // Create the table client.
            //CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            //CloudTable table = tableClient.GetTableReference("Coupons");

            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.Load(outletsLink);

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
                product1.产品名称 = storeName;
                product1.开始日期 = startDate;
                product1.结束日期 = endDate;
                product1.产品描述 = eventDescription;

                // Create the TableOperation that inserts the customer entity.
                TableOperation insertOperation = TableOperation.Insert(product1);

                // Execute the insert operation.
                try
                {
                    table.Execute(insertOperation);
                    couponsArchive.Execute(insertOperation);
                }
                catch (Exception e)
                {
                    continue;
                }
            }
        }
    }
}
