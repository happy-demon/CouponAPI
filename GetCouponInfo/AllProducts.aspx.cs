using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.Net;

public partial class AllProducts : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var url = "http://couponapi.azurewebsites.net/api/coupons";
        var allProducts = _download_serialized_json_data<Product>(url);
        foreach (var x in allProducts)
        {
            Response.Write("<img src=" + x.ProductPicture + ">" + "<BR/>");
            Response.Write("产品名称： " + x.ProductName + "<BR/>");
            Response.Write("产品类别： " + x.ProductCategory + "<BR/>");
            Response.Write("产品链接： " + x.ProductLink + "<BR/>");
            Response.Write("起始日期： " + x.StartDate + "<BR/>");
            Response.Write("结束日期： " + x.EndDate + "<BR/>");
            Response.Write("折扣价格： " + x.OfferPrice + "<BR/>");
            Response.Write("<BR/>");
        }

    }
    private static List<T> _download_serialized_json_data<T>(string url)
    {
        using (var w = new WebClient())
        {
            var json_data = string.Empty;
            // attempt to download JSON data as a string
            try
            {
                json_data = w.DownloadString(url);
            }
            catch (Exception) { }
            // if string with JSON data is not empty, deserialize it to class and return its instance 
            return JsonConvert.DeserializeObject<List<T>>(json_data);
        }
    }
    public class Product
    {
        public string ProductCategory { get; set; }
        public string ProductName { get; set; }
        public object ItemNumber { get; set; }
        public string ProductLink { get; set; }
        public string ProductPicture { get; set; }
        public string ProductDescription { get; set; }
        public string ProductDescription_cn { get; set; }
        public string OriginalPrice { get; set; }
        public object LessPrice { get; set; }
        public string OfferPrice { get; set; }
        public object StartDate { get; set; }
        public object EndDate { get; set; }
        public object ProductReview { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime Timestamp { get; set; }
        public string ETag { get; set; }
    }
}