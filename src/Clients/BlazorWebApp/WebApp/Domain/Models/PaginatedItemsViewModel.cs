namespace WebApp.Domain.Models
{
    public class PaginatedItemsViewModel<T> where T : class
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public long Count { get; set; }
        public IEnumerable<T> Items { get; set; }
        public PaginatedItemsViewModel(int pageIndex, int pageSize, long count, IEnumerable<T> items)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            Count = count;
            Items = items;
        }

        public PaginatedItemsViewModel()
        {

        }
    }
}
