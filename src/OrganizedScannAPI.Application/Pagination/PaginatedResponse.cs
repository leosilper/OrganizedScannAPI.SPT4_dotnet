using System.Collections.Generic;

namespace OrganizedScannAPI.Application.Pagination
{
    public class PaginatedResponse<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public int TotalPages => (Total + PageSize - 1) / PageSize;
        public List<T> Items { get; set; } = new();
    }
}
