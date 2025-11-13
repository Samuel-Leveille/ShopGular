using System.Collections.Generic;

namespace ShopGular.Backend.Models.Dtos;

public record PagedProductsDto(
    IReadOnlyList<ProductDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

