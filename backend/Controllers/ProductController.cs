using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ShopGular.Backend.Models.Dtos;
using ShopGular.Backend.Services;

namespace ShopGular.Backend.Controllers;

[ApiController]
[Route("api/user/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductCatalogService _productCatalogService;

    public ProductController(ProductCatalogService productCatalogService)
    {
        _productCatalogService = productCatalogService;
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductDetailDto>> GetById(long id, CancellationToken cancellationToken = default)
    {
        var product = await _productCatalogService.GetProductDetailAsync(id, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpGet("by-category")]
    public async Task<ActionResult<IReadOnlyList<CategoryProductsDto>>> GetByCategory([FromQuery] int limit = 4, CancellationToken cancellationToken = default)
    {
        var blocks = await _productCatalogService.GetProductsGroupedByCategoryAsync(limit, cancellationToken);
        return Ok(blocks);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedProductsDto>> Search([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(new PagedProductsDto(Array.Empty<ProductDto>(), 0, Math.Max(page, 1), Math.Clamp(pageSize, 1, 50)));
        }

        var result = await _productCatalogService.SearchProductsAsync(q, page, pageSize, cancellationToken);
        return Ok(result);
    }
}

