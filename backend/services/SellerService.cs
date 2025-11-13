using Microsoft.EntityFrameworkCore;
using ShopGular.backend.Models;
using ShopGular.Backend.Models;
using ShopGular.Backend.Models.Dtos;
using System.Linq;

namespace ShopGular.Backend.Services;

public class SellerService
{
    private readonly AppDbContext _context;

    public SellerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SellerDto?> GetSellerByIdAsync(long id)
    {
        var seller = await _context.Sellers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        return seller != null ? Seller.ToDto(seller) : null;
    }

    public async Task<ProductDto?> AddProductAsync(long sellerId, CreateProductDto dto)
    {
        var sellerExists = await _context.Sellers.AnyAsync(s => s.Id == sellerId);
        if (!sellerExists)
        {
            return null;
        }

        var entity = Product.ToEntityNewProduct(dto, sellerId);
        _context.Products.Add(entity);
        await _context.SaveChangesAsync();
        return Product.ToDto(entity);
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsForSellerAsync(long sellerId)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.SellerId == sellerId)
            .OrderByDescending(p => p.DateOfSale)
            .ToListAsync();

        return products.Select(Product.ToDto).ToList();
    }

    public async Task<SellerDto?> SignUpAsync(SignUpSellerDto dto)
    {
        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
        {
            return null;
        }

        var seller = Seller.SignUpDtoToEntity(dto);
        seller.Password = PasswordHashing.HashPassword(dto.Password);
        _context.Sellers.Add(seller);
        await _context.SaveChangesAsync();
        return Seller.ToDto(seller);
    }

    public async Task<ProductDto?> UpdateProductAsync(long sellerId, long productId, UpdateProductDto dto)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && p.SellerId == sellerId);
        if (product == null)
        {
            return null;
        }

        product.Title = dto.Title;
        product.Description = dto.Description;
        product.Quantity = dto.Quantity;

        if (!string.IsNullOrWhiteSpace(dto.ImagePath))
        {
            product.Image = dto.ImagePath!;
        }

        await _context.SaveChangesAsync();

        return Product.ToDto(product);
    }
}
