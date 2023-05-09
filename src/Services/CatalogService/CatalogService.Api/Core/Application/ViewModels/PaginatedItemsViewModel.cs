namespace CatalogService.Api.Core.Application.ViewModels
{
    public class PaginatedItemsViewModel<TEntity> where TEntity : class
    {

        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public long Count { get; private set; }
        public IEnumerable<TEntity> Items { get; set; }
        public PaginatedItemsViewModel(int pageIndex, int pageSize, long count, IEnumerable<TEntity> items)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            Count = count;
            Items = items;
        }
    }
}
