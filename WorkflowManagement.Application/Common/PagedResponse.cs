namespace WorkflowManagement.Application.Common
{
    /// <summary>
    /// Wraps paginated API responses with metadata
    /// so clients know total count and navigation info.
    /// </summary>
    public class PagedResponse<T>
    {
        /// <summary>
        /// The page of data items.
        /// </summary>
        public IEnumerable<T> Data { get; set; }
            = Enumerable.Empty<T>();

        /// <summary>
        /// Current page number (1-based).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of records in the full dataset.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int TotalPages =>
            PageSize == 0
                ? 0
                : (int)Math.Ceiling(
                    (double)TotalCount / PageSize);

        /// <summary>
        /// True if there is a page after this one.
        /// </summary>
        public bool HasNextPage =>
            PageNumber < TotalPages;

        /// <summary>
        /// True if there is a page before this one.
        /// </summary>
        public bool HasPreviousPage =>
            PageNumber > 1;

        public PagedResponse(
            IEnumerable<T> data,
            int pageNumber,
            int pageSize,
            int totalCount)
        {
            Data = data;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }
}
