using ShopGular.Backend.Models.Dtos;
using ShopGular.Backend.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ShopGular.Backend.Models;
public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; private set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public double Price { get; set; }
    public string Category { get; set; }
    public string Image { get; set; }
    public List<int>? Ratings { get; set; }
    public int Quantity { get; set; }
    public ProductTag Tag { get; set; }
    public int PurchaseQuantity { get; set; }
    public DateTime DateOfSale { get; private set; }

    public Product(string title, string description, double price, string category, string image, int quantity, int purchaseQuantity, ProductTag tag)
    {
        Title = title;
        Description = description;
        Price = price;
        Category = category;
        Image = image;
        Tag = tag;
        Quantity = quantity;
        DateOfSale = DateTime.Now;
        PurchaseQuantity = purchaseQuantity;
    }

    public static Product ToEntityNewProduct(CreateProductDto dto)
    {
        return new Product(dto.Title, dto.Description, dto.Price, dto.Category, dto.Image, dto.Quantity, dto.PurchaseQuantity, dto.Tag);
    }

    public static ProductDto ToDto(Product product)
    {
        return new ProductDto(product.Id, product.Title, product.Description, product.Price, product.Category, product.Image, product.Quantity, product.PurchaseQuantity, product.Tag, product.DateOfSale);
    }
}