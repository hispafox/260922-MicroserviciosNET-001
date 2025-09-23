using System.Collections.Generic;

namespace Menu.Infrastructure.Persistence
{
    public class PagedTokenResult<T>
    {
        public IReadOnlyList<T> Items { get; set; }
        public string? NextToken { get; set; }
        public int PageSize { get; set; }
    }
}
