using ShopGular.Backend.Models.Dtos;
using ShopGular.Backend.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ShopGular.backend.Models;

namespace ShopGular.Backend.Models;

public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; private set; }
    public long SellerId { get; set; }
    public Seller? Seller { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public List<int>? Ratings { get; set; } = new();
    public int Quantity { get; set; }
    public ProductTag Tag { get; set; }
    public int PurchaseQuantity { get; set; }
    public DateTime DateOfSale { get; private set; }
    public Client? BoughtByClient { get; set; }
    public long? BoughtByClientFK { get; set; }
    public Client? InShoppingCartByClient { get; set; }
    public long? InShoppingCartByClientFK { get; set; }

    public Product()
    {
        DateOfSale = DateTime.UtcNow;
    }

    public Product(string title, string description, double price, string category, string? image, int quantity, int purchaseQuantity, ProductTag tag, long sellerId, DateTime? createdAt = null)
    {
        Title = title;
        Description = description;
        Price = price;
        Category = category;
        Image = image ?? string.Empty;
        Tag = tag;
        Quantity = quantity;
        SellerId = sellerId;
        DateOfSale = createdAt ?? DateTime.UtcNow;
        PurchaseQuantity = purchaseQuantity;
    }

    public static Product ToEntityNewProduct(CreateProductDto dto, long sellerId)
    {
        return new Product(
            dto.Title,
            dto.Description,
            dto.Price,
            dto.Category,
            dto.Image,
            dto.Quantity,
            dto.PurchaseQuantity ?? 0,
            dto.Tag ?? ProductTag.InStock,
            sellerId,
            dto.DateOfSale);
    }

    public static ProductDto ToDto(Product product)
    {
        return new ProductDto(
            product.Id,
            product.Title,
            product.Description,
            product.Price,
            product.Category,
            product.Image,
            product.Quantity,
            product.PurchaseQuantity,
            product.Tag,
            product.DateOfSale,
            product.SellerId);
    }

    public static ProductDetailDto ToDetailDto(Product product)
    {
        return new ProductDetailDto(
            product.Id,
            product.Title,
            product.Description,
            product.Price,
            product.Category,
            product.Image,
            product.Quantity,
            product.PurchaseQuantity,
            product.Tag,
            product.DateOfSale,
            product.SellerId,
            product.Seller?.Name ?? string.Empty,
            product.Seller?.Email ?? string.Empty);
    }
}