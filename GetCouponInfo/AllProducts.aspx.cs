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
        var url = "http://coderwall.com/mdeiters.json";
        var allProducts = _download_serialized_json_data<Product>(url);
        Response.Write(allProducts.ProductId);
    }
    private static T _download_serialized_json_data<T>(string url) where T : new()
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
            return !string.IsNullOrEmpty(json_data) ? JsonConvert.DeserializeObject<T>(json_data) : new T();
        }
    }
    public class Product
    {
        public string StoreName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Event { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime Timestamp { get; set; }
        public string ETag { get; set; }
    }
}