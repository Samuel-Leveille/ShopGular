using System.Collections.Generic;

namespace ShopGular.Backend.Models.Dtos;

public record CategoryProductsDto(
    string Category,
    IReadOnlyList<ProductDto> Products);

