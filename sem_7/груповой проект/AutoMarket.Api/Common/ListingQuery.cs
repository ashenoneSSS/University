namespace AutoMarket.Api.Common;

/// <summary>Варіанти сортування каталогу.</summary>
public enum ListingSort
{
    Newest,
    Oldest,
    PriceAsc,
    PriceDesc,
    Popular
}

/// <summary>Фільтри пошуку оголошень для гостей.</summary>
public class ListingQuery : PageRequest
{
    /// <summary>Текст для пошуку в назві та описі.</summary>
    public string? Q { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? City { get; set; }
    public ListingSort Sort { get; set; } = ListingSort.Newest;
}
