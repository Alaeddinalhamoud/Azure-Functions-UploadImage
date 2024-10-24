namespace ShopV2.Web.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public List<Image>? ImageUrl { get; set; }
}
