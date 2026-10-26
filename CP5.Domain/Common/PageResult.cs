namespace CP5.Domain.Common
{
    public class PageResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalItems { get; }
        public int TotalPages { get; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PageResult(IEnumerable<T> items, int totalItems, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            PageSize = pageSize < 1 ? 10 : pageSize;
            TotalItems = totalItems < 0 ? 0 : totalItems;
            TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
            Items = items?.ToList().AsReadOnly() ?? new List<T>().AsReadOnly();
        }

        public static PageResult<T> Create(IEnumerable<T> items, int totalItems, int pageNumber, int pageSize)
            => new(items, totalItems, pageNumber, pageSize);
    }
}
