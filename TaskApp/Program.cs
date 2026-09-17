using System.Globalization;
using System.Text.Json;
using HtmlAgilityPack;

class Product
{
    public string ProductName { get; set; }
    public string Price { get; set; }
    public string Rating { get; set; }
}

class Program
{
    static void Main()
    {
        var doc = new HtmlDocument();
        doc.Load(@"Targetroute\index.html");

        var products = new List<Product>();
        var itemNodes = doc.DocumentNode.SelectNodes("//div[@class='item']")!;

        foreach (var item in itemNodes)
        {
            var productNode = item.SelectSingleNode(".//img");
            string productName = HtmlEntity.DeEntitize(productNode?.GetAttributeValue("alt", "").Trim());

            var priceNode = item.SelectSingleNode(".//span[@style='display: none']");
            var rawPrice = priceNode.InnerText;


            var rawRating = item.GetAttributeValue("rating", "").Trim();

            var price = FormatPrice(rawPrice);
            var rating = FormatRating(rawRating);

            products.Add(new Product
            {
                ProductName = productName,
                Price = price,
                Rating = rating
            });
        }

        var result = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        Console.WriteLine(JsonSerializer.Serialize(products, result));

        
        
        static string FormatPrice(string rawPrice)
        {
            if (string.IsNullOrWhiteSpace(rawPrice)) return null;
            return rawPrice.Replace("$", "").Trim().Replace(",", "").Trim();
        }

        static string FormatRating(string rawRating)
        {
            if (string.IsNullOrWhiteSpace(rawRating)) return null;

            var value = double.Parse(rawRating, CultureInfo.InvariantCulture);
            var rating = value > 5 ? value / 2 : value;
            rating = Math.Round(rating, 2);

            return rating.ToString(CultureInfo.InvariantCulture);
        }
    }
}