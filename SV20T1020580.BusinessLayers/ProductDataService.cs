using SV20T1020580.DataLayers;
using SV20T1020580.DataLayers.SQLServer;
using SV20T1020580.DomainModels;
using System.Security.Authentication;

namespace SV20T1020580.BusinessLayers
{
    public static class ProductDataService
    {
        private static readonly IProdcutDAL productDB;
        /// <summary>
        /// Ctor
        /// </summary>
        static ProductDataService()
        {
            productDB = new ProductDAL(Configuration.ConnectionString);
        }
        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng (không phân trang)
        /// </summary>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public static List<Product> ListProducts(string searchValue)
        {
            return productDB.List().ToList();
        }
        /// <summary>
        /// Tìm kiếm và lấy dánh sách mặt hàng dưới dạng phân trang
        /// </summary>
        /// <param name="rowCount"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchValue"></param>
        /// <param name="categoryId"></param>
        /// <param name="supplierId"></param>
        /// <param name="minPrice"></param>
        /// <param name="maxPrice"></param>
        /// <returns></returns>
        public static List<Product> ListProducts(out int rowCount, int page = 1, int pageSize = 0,
            string searchValue = "",
            int categoryId = 0, int supplierId = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            rowCount = productDB.Count(searchValue, categoryId, supplierId, minPrice, maxPrice);
            return productDB.List(page, pageSize, searchValue, categoryId, supplierId, minPrice, maxPrice).ToList();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static Product? GetProduct(int productId)
        {
            return productDB.Get(productId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int AddProduct(Product data)
        {
            return productDB.Add(data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool UpdateProduct(Product data)
        {
            return productDB.Update(data);
        }
        /// <summary>
        /// Xóa mặt hàng
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool DeleteProduct(int productId)
        {
            if (productDB.IsUsed(productId))
                return false;

            return productDB.Delete(productId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool InUsedProduct(int productId)
        {
            return productDB.IsUsed(productId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static List<ProductPhoto> ListPhotos(int productId)
        {
            return productDB.ListPhotos(productId).ToList();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="photoId"></param>
        /// <returns></returns>
        public static ProductPhoto? GetPhoto(long photoId)
        {
            return productDB.GetPhoto(photoId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static long AddPhoto(ProductPhoto data)
        {
            return productDB.AddPhoto(data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool UpdatePhoto(ProductPhoto data)
        {
            return productDB.UpdatePhoto(data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="photoId"></param>
        /// <returns></returns>
        public static bool DeletePhoto(long photoId)
        {
            return productDB.DeletePhoto(photoId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static List<ProductAttribute> ListAttributes(int productId)
        {
            return productDB.ListAttributes(productId).ToList();
        }

        public static ProductAttribute? GetAttribute(int attributeId)
        {
            return productDB.GetAttribute(attributeId);
        }
        public static long AddAttribute(ProductAttribute data)
        {
            return productDB.AddAttribute(data);
        }
        public static bool UpdateAttribute(ProductAttribute data)
        {

            return productDB.UpdateAttribute(data);
        }
        public static bool DeleteAttribute(long attributeId)
        {
            return productDB.DeleteAttribute(attributeId);
        }
    }
}
