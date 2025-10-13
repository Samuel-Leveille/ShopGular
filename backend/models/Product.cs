namespace ShopGular.Backend.Models;
public class Product {
    public string id { get; private set; }
    public string title { get; set; }
    public string description {get; set; }
    public double price { get; set; }
    public string category { get; set; }
    public string image { get; set; }
    public List<int> ratings { get; set; }
    public int quantity { get; set; }
    public ProductTag tag { get; set; }
    public int purchaseQuantity { get; set; }
    public DateTime dateOfSale { get; private set; }

    public Product() {
        dateOfSale = DateTime.Now;
    }

    public Product(string productTitle, string productDescription, double productPrice, string productCategory, string productImage, int productQuantity, ProductTag productTag) {
        product = productTitle;
        description = productDescription;
        price = productPrice;
        category = productCategory;
        image = productImage;
        quantity = productQuantity;
        tag = productTag;
        purchaseQuantity = 0;
        dateOfSale = DateTime.Now;
    }
}