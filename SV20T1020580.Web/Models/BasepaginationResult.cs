using SV20T1020580.DomainModels;

namespace SV20T1020580.Web.Models
{
    /// <summary>
    /// lớp cơ siwr cho các lớp biểu diễn dữ liệu là kết quả của thao tác
    /// tìm kiếm, phân trang
    /// </summary>
    public abstract class BasepaginationResult
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SearchValue { get; set; } = "";
        public int RowCount { get; set; }
        public int PageCount
        {
            get
            {
                if (PageSize == 0)
                    return 1;

                int c = RowCount / PageSize;
                if (RowCount % PageSize > 0)
                    c += 1;
                return c;

            }
        }

    }

    /// <summary>
    /// Kết quả tìm kiếm và lấy danh sách khách hàng
    /// </summary>
    public class CustomerSearchResult : BasepaginationResult
    {
        public List<Customer> Data { get; set; } = new List<Customer>();
    }
    /// <summary>
    /// Kết quả tìm kiếm và lấy danh sách người giao hàng
    /// </summary>
    public class ShipperSearchResult : BasepaginationResult
    {
        public List<Shipper> Data { get; set; } = new List<Shipper>();
    }
    /// <summary>
    /// Kết quả tìm kiếm và lấy danh sách loại hàng
    /// </summary>
    public class CategorySearchResult : BasepaginationResult
    {
        public List<Category> Data { get; set; } = new List<Category>();
    }

    public class SupplierSearchResult : BasepaginationResult
    {
        public List<Supplier> Data { get; set; } = new List<Supplier>();
    }

    public class EmployeeSearchResult : BasepaginationResult
    {
        public List<Employee> Data { get; set; } = new List<Employee>();
    }

    public class ProductSearchResult : BasepaginationResult
    {
        public List<Product> Data { get; set; } = new List<Product>();
    }
}
