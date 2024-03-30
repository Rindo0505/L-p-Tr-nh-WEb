using SV20T1020580.DomainModels;

namespace SV20T1020580.Web.Models
{
    public class OrderSearchResult : BasepaginationResult
    {
        public int Status { get; set; } = 0;
        public string TimeRange { get; set; } = "";
        public List<Order> Data { get; set; } = new List<Order>();
    }
    /*Lớp OrderDetailModel: biểu diễn dữ liệu sử dụng cho chức năng hiển thị chi tiết của đơn hàng
    (Order/Details):*/
public class OrderDetailModel
    {
        public Order Order { get; set; }
        public List<OrderDetail> Details { get; set; }
    }
}
