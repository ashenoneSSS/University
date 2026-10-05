namespace AutoMarket.Api.Common;

/// <summary>Сторінка результатів з метаданими пагінації.</summary>
public class PagedResult<T>
{
    public required List<T> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}

/// <summary>Параметри пагінації з обмеженням розміру сторінки.</summary>
public class PageRequest
{
    public const int MaxPageSize = 100;
    private int _page = 1;
    private int _pageSize = 20;

    /// <summary>Номер сторінки (від 1).</summary>
    public int Page { get => _page; set => _page = value < 1 ? 1 : value; }

    /// <summary>Розмір сторінки (1–100).</summary>
    public int PageSize { get => _pageSize; set => _pageSize = Math.Clamp(value, 1, MaxPageSize); }

    public int Skip => (Page - 1) * PageSize;
}
