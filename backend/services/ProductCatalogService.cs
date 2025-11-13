using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShopGular.Backend;
using ShopGular.Backend.Models;
using ShopGular.Backend.Models.Dtos;

namespace ShopGular.Backend.Services;

public class ProductCatalogService
{
    private readonly AppDbContext _context;

    public ProductCatalogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoryProductsDto>> GetProductsGroupedByCategoryAsync(int limitPerCategory = 4, CancellationToken cancellationToken = default)
    {
        if (limitPerCategory <= 0)
        {
            limitPerCategory = 4;
        }

        var products = await _context.Products
            .AsNoTracking()
            .OrderByDescending(p => p.PurchaseQuantity)
            .ThenByDescending(p => p.DateOfSale)
            .ToListAsync(cancellationToken);

        var grouped = products
            .Where(p => !string.IsNullOrWhiteSpace(p.Category))
            .GroupBy(p => p.Category.Trim())
            .Select(group => new CategoryProductsDto(
                group.Key,
                group
                    .Take(limitPerCategory)
                    .Select(Product.ToDto)
                    .ToList()
            ))
            .OrderByDescending(g => g.Products.Count)
            .ThenBy(g => g.Category)
            .ToList();

        return grouped;
    }

    public async Task<PagedProductsDto> SearchProductsAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedQuery = query?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return new PagedProductsDto(Array.Empty<ProductDto>(), 0, page, pageSize);
        }

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var skip = (page - 1) * pageSize;

        var baseQuery = _context.Products.AsNoTracking()
            .Where(p => EF.Functions.ILike(p.Title, $"%{normalizedQuery}%"))
            .OrderByDescending(p => p.DateOfSale)
            .ThenBy(p => p.Title);

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(Product.ToDto).ToList();

        return new PagedProductsDto(dtos, totalCount, page, pageSize);
    }

    public async Task<ProductDetailDto?> GetProductDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return product != null ? Product.ToDetailDto(product) : null;
    }
}

