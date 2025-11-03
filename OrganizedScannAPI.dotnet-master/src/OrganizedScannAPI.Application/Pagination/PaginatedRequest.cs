namespace OrganizedScannAPI.Application.Pagination
{
    public class PaginatedRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public const int MaxPageSize = 100;

        public void Normalize()
        {
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 1;
            if (PageSize > MaxPageSize) PageSize = MaxPageSize;
        }
    }
}
