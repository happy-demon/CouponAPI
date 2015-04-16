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
        public class URL : TableEntity
        {
            public URL(string name, string id)
            {
                this.PartitionKey = name;
                this.RowKey = id;
            }
            public URL() { }
            public string link { get; set; }
        }
        public class Costco : TableEntity
        {
            public Costco(string category, string guid)
            {
                this.PartitionKey = category;
                this.RowKey = guid;
            }
            public Costco() { }
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
        public class Macys : TableEntity
        {
            public Macys(string category, string guid)
            {
                this.PartitionKey = category;
                this.RowKey = guid;
            }
            public Macys() { }
            public string ProductName { get; set; }
            public string ProductCategory { get; set; }
            public string ProductLink { get; set; }
            public string ProductPicture { get; set; }
            public string ProductDescription { get; set; }
            public string ProductDescription_cn { get; set; }
            public string OriginalPrice { get; set; }
            public string OfferPrice { get; set; }
        }
        public class Outlets : TableEntity
        {
            public Outlets(string category, string guid)
            {
                this.PartitionKey = category;
                this.RowKey = guid;
            }
            public Outlets() { }
            public string ProductCategory { get; set; }
            public string ProductName { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string ProductDescription { get; set; }
            public string ProductDescription_cn { get; set; }
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
            CloudTable url = tableClient.GetTableReference("URL");
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

            TableQuery<URL> query_Outlets = new TableQuery<URL>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Outlets"));
            TableQuery<URL> query_Costco = new TableQuery<URL>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Costco"));
            TableQuery<URL> query_Macys = new TableQuery<URL>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Macys"));

            foreach (URL entity in url.ExecuteQuery(query_Outlets))
            {
                try
                {
                    OutletsFetcher(entity.link);
                }
                catch (Exception e)
                {
                    Console.WriteLine(entity.link);
                    Console.WriteLine(e.Message);
                    continue;
                }
            }

            foreach (URL entity in url.ExecuteQuery(query_Costco))
            {
                try
                {
                    CostcoFetcher(entity.link);
                }
                catch (Exception e)
                {
                    Console.WriteLine(entity.link);
                    Console.WriteLine(e.Message);
                    continue;
                }
            }

            foreach (URL entity in url.ExecuteQuery(query_Macys))
            {
                try
                {
                    MacysFetcher(entity.link);
                }
                catch (Exception e)
                {
                    Console.WriteLine(entity.link);
                    Console.WriteLine(e.Message);
                    continue;
                }
            }
        }
        public static void CostcoFetcher(string link)
        {
            var categoryName = "";
            var title = "";
            var rowKey = "";
            var img = "";
            var des = "";
            var des_cn = "";
            var online_price = "";
            var less_price = "";
            var your_price = "";
            var startDate = "";
            var endDate = "";
            var review = "";
            var costcoLink = link;

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
                categoryName = product_col.SelectSingleNode(".//a[@href]").Attributes["title"].Value.Replace("&amp;", "&").Replace("&#039;", "'");

                HtmlDocument htmlDocument1 = htmlWeb.Load(col_link);
                var products = htmlDocument1.DocumentNode.SelectNodes("//*[contains(@class,'product-tile-image-container')]");

                foreach (var product in products)
                {
                    rowKey = Guid.NewGuid().ToString();
                    var productInfo = product.SelectSingleNode(".//a[@href]");
                    var productLink = productInfo.Attributes["href"].Value;
                    var itemNumber = productLink.Split('.')[productLink.Split('.').Count() - 2];

                    foreach (var x in productInfo.SelectNodes(".//*[contains(@class,'short-desc')]"))
                    {
                        title = x.InnerText.Trim();
                    }

                    foreach (var x in productInfo.SelectNodes(".//img"))
                    {
                        img = x.Attributes["src"].Value;
                        des = x.Attributes["alt"].Value;
                        try
                        {
                            des_cn = TranslateText(des);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(productLink);
                            Console.WriteLine(e.Message);
                            continue;
                        }
                    }

                    try
                    {
                        HtmlDocument htmlDocument2 = htmlWeb.Load(productLink);
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
                        Console.WriteLine(productLink);
                        Console.WriteLine(e.Message);
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

                    /* Console.WriteLine(title);
                    Console.WriteLine(itemNumber);
                    Console.WriteLine(productLink);
                    Console.WriteLine(img);
                    Console.WriteLine(des);
                    Console.WriteLine(online_price);
                    Console.WriteLine(less_price);
                    Console.WriteLine(your_price);
                    Console.WriteLine(startDate);
                    Console.WriteLine(endDate);
                    Console.WriteLine(review);
                    Console.WriteLine(rowKey); */
                    //Console.ReadLine();

                    // Create a new customer entity.
                    Costco product1 = new Costco("Costco", rowKey);
                    product1.ProductCategory = categoryName;
                    product1.ProductName = title;
                    product1.ItemNumber = itemNumber;
                    product1.ProductLink = productLink;
                    product1.ProductPicture = img;
                    product1.ProductDescription = des;
                    product1.ProductDescription_cn = des_cn;
                    product1.OriginalPrice = online_price;
                    product1.LessPrice = less_price;
                    product1.OfferPrice = your_price;
                    product1.StartDate = startDate;
                    product1.EndDate = endDate;
                    product1.ProductReview = review;

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
                        Console.WriteLine(productLink);
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }
            }
        }
        public static void MacysFetcher(string link)
        {
            var categoryName = "";
            var title = "";
            var productLink = "";
            var img = "";
            var rowKey = "";
            var originalPrice = "";
            var offerPrice = "";
            var des = "";
            var des_cn = "";
            var macysLink = link;

            // Retrieve the storage account from the connection string.
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            //    ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            // Create the table client.
            //CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            //CloudTable table = tableClient.GetTableReference("Coupons");

            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.Load(macysLink);

            categoryName = htmlDocument.DocumentNode.SelectSingleNode("//h1[@class = 'currentCategory']").InnerText;

            var categories = htmlDocument.DocumentNode.
                SelectSingleNode("//ul[@class = 'thumbnails large-block-grid-4' and @id = 'thumbnails']");

            foreach (var product in categories.SelectNodes(".//div[@class = 'innerWrapper']"))
            {
                des = "";
                rowKey = Guid.NewGuid().ToString();
                productLink = "http://www1.macys.com" + product.SelectSingleNode(".//a[@href]").Attributes["href"].Value;

                try
                {
                    title = product.SelectSingleNode(".//img").Attributes["title"].Value;
                    img = product.SelectSingleNode(".//img").Attributes["data-src"].Value;
                }
                catch (Exception e)
                {
                    Console.WriteLine(productLink);
                    Console.WriteLine(e.Message);
                    continue;
                }

                HtmlDocument htmlDocument2 = htmlWeb.Load(productLink);
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

                HtmlDocument htmlDocument3 = htmlWeb.Load(productLink);
                var giftOffers = htmlDocument3.DocumentNode.SelectNodes("//div[(@class = 'giftOfferDetails')]");
                if (giftOffers == null)
                {
                    des = "";
                    des_cn = "";
                }
                else
                {
                    foreach (var x in giftOffers)
                    {
                        des += x.SelectSingleNode(".//div[(@class = 'giftOfferDescription')]").InnerText;
                        try
                        {
                            des_cn = TranslateText(des);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(productLink);
                            Console.WriteLine(e.Message);
                            continue;
                        }
                    }
                }

                /* Console.WriteLine(title);
                Console.WriteLine(rowKey);
                Console.WriteLine(originalPrice);
                Console.WriteLine(offerPrice);
                Console.WriteLine(productLink);
                Console.WriteLine(img);
                Console.WriteLine(des); */
                //Console.ReadLine();

                Macys product1 = new Macys("Macys", rowKey);
                product1.ProductName = title;
                product1.ProductCategory = categoryName;
                product1.ProductLink = productLink;
                product1.ProductPicture = img;
                product1.OriginalPrice = originalPrice.Trim();
                product1.OfferPrice = offerPrice.Trim();
                product1.ProductDescription = des;
                product1.ProductDescription_cn = des_cn;

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
                    Console.WriteLine(productLink);
                    Console.WriteLine(e.Message);
                    continue;
                }
            }
        }
        public static void OutletsFetcher(string link)
        {
            var categoryName = "";
            var storeName = "";
            var startDate = "";
            var rowKey = "";
            var endDate = "";
            var eventDescription = "";
            var eventDescription_cn = "";
            var outletsLink = link;

            // Retrieve the storage account from the connection string.
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            //    ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            // Create the table client.
            //CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            //CloudTable table = tableClient.GetTableReference("Coupons");

            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.Load(outletsLink);

            categoryName = htmlDocument.DocumentNode.SelectSingleNode("//h1[@class = 'title mb-10']").InnerText.Trim().Replace("&reg;", "");

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
                try
                {
                    eventDescription_cn = TranslateText(eventDescription);
                }
                catch (Exception e)
                {
                    Console.WriteLine(outletsLink);
                    Console.WriteLine(storeName);
                    Console.WriteLine(e.Message);
                    continue;
                }

                try
                {
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
                }
                catch (Exception e)
                {
                    Console.WriteLine(outletsLink);
                    Console.WriteLine(storeName);
                    Console.WriteLine(e.Message);
                    continue;
                }

                /* Console.WriteLine(storeName);
                Console.WriteLine(startDate);
                Console.WriteLine(endDate);
                Console.WriteLine(eventDescription); */
                //Console.ReadLine();

                // Create a new customer entity.
                Outlets product1 = new Outlets("Outlets", rowKey);
                product1.ProductCategory = categoryName;
                product1.ProductName = storeName;
                product1.StartDate = startDate;
                product1.EndDate = endDate;
                product1.ProductDescription = eventDescription;
                product1.ProductDescription_cn = eventDescription_cn;

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
                    Console.WriteLine(outletsLink);
                    Console.WriteLine(storeName);
                    Console.WriteLine(e.Message);
                    continue;
                }
            }
        }
        public static string TranslateText(string input)
        {
            string url = String.Format("http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair=en|zh", input);

            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.Load(url);

            var result = htmlDocument.DocumentNode.
                SelectSingleNode("//span[@id = 'result_box']");

            return result.InnerText;
        }
    }
}
