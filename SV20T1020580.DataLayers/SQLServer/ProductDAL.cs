using Azure;
using Dapper;
using SV20T1020580.DomainModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV20T1020580.DataLayers.SQLServer
{
    public class ProductDAL : _BaseDAL, IProdcutDAL
    {
        public ProductDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Product data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"if exists(select * from Products where ProductName = @ProductName)
                                select -1
                            else
                                begin
                                    insert into Products(ProductName,ProductDescription,SupplierID,CategoryID,Unit,Price,Photo,IsSelling)
                                    values(@productName,@productDescription,@supplierID,@categoryID,@unit,@price,@photo,@isSelling);
                                    select @@identity;
                                end";

                var parameters = new
                {
                    productName = data.ProductName ?? "",
                    productDescription = data.ProductDescription ?? "",
                    supplierId = data.SupplierId,
                    categoryId = data.CategoryId,
                    unit = data.Unit ?? "",
                    price = data.Price,
                    photo = data.Photo ?? "",
                    isSelling = data.IsSelling
                };

                id = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);

                connection.Close();
            }

            return id;
        }

        public long AddAttribute(ProductAttribute data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"if exists(select * from ProductAttributes where ProductID = @ProductID and AttributeName = @AttributeName)
                                select -1
                            else
                                begin
                                    insert into ProductAttributes(ProductID,AttributeName,AttributeValue,DisplayOrder)
                                    values(@ProductID,@AttributeName,@AttributeValue,@DisplayOrder);
                                    select @@identity;
                                end";

                var parameters = new
                {
                    ProductID = data.ProductId,
                    AttributeName = data.AttributeName ?? "",
                    AttributeValue = data.AttributeValue ?? "",
                    DisplayOrder = data.DisplayOrder
                };

                id = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);

                connection.Close();
            }

            return id;
        }

        public long AddPhoto(ProductPhoto data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"insert into ProductPhotos(ProductID,Photo,Description,DisplayOrder,IsHidden)
                                values(@ProductID,@Photo,@Description,@DisplayOrder,@IsHidden);
                            select @@identity;";

                var parameters = new
                {
                    ProductID = data.ProductId,
                    Photo = data.Photo ?? "",
                    Description = data.Description ?? "",
                    DisplayOrder = data.DisplayOrder,
                    IsHidden = data.IsHidden
                };

                id = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);

                connection.Close();
            }

            return id;
        }

        public int Count(string searchValue = "", int categoryId = 0, int supplierId = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            int count = 0;
            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";

            using (var connection = OpenConnection())
            {
                var sql = @"select count(*) 
                            from Products
                            where   (@searchValue = N'' or ProductName like @searchValue)
                                and (@CategoryID = 0 or CategoryID = @CategoryID)
                                and (@SupplierID = 0 or SupplierID = @SupplierID)
                                and (Price >= @MinPrice)
                                and (@MaxPrice <= 0 or Price <= @MaxPrice)";

                var parameters = new
                {
                    searchValue = searchValue,
                    CategoryID = categoryId,
                    SupplierID = supplierId,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };

                count = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return count;
        }

        public bool Delete(int productId)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                string sql = @"if not exists(select * from OrderDetails where ProductID = @productID)
	                        begin
		                        delete from Products where ProductID = @productID
		                        delete from ProductPhotos where ProductID = @productID
		                        delete from ProductAttributes where ProductID = @productID
	                        end;";

                var parameters = new { productID = productId };

                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;

                connection.Close();
            }

            return result;
        }

        public bool DeleteAttribute(long attributeId)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"delete from ProductAttributes where AttributeID = @attributeID";

                var parameters = new { attributeID = attributeId };

                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;

                connection.Close();
            }

            return result;
        }

        public bool DeletePhoto(long photoId)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"delete from ProductPhotos where PhotoID = @photoID";

                var parameters = new { photoID = photoId };

                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;

                connection.Close();
            }

            return result;
        }

        public Product? Get(int productId)
        {
            Product? data = null;

            using (var connection = OpenConnection())
            {
                var sql = "select * from Products where ProductID = @productID";

                var parameters = new { productID = productId };

                data = connection.QueryFirstOrDefault<Product>(sql: sql, param: parameters, commandType: CommandType.Text);

                connection.Close();
            }

            return data;
        }

        public ProductAttribute? GetAttribute(long attributeId)
        {
            ProductAttribute? data = null;
            using (var connection = OpenConnection())
            {
                var sql = "select * from ProductAttributes where AttributeID = @attributeID";

                var parameters = new { attributeID = attributeId };

                data = connection.QueryFirstOrDefault<ProductAttribute>(sql: sql, param: parameters, commandType: CommandType.Text);

                connection.Close();
            }

            return data;
        }

        public ProductPhoto? GetPhoto(long photoId)
        {
            ProductPhoto? data = null;
            using (var connection = OpenConnection())
            {
                var sql = "select * from ProductPhotos where PhotoID = @photoID";

                var parameters = new { photoID = photoId };

                data = connection.QueryFirstOrDefault<ProductPhoto>(sql: sql, param: parameters, commandType: CommandType.Text);

                connection.Close();
            }

            return data;
        }

        public bool IsUsed(int productId)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"if exists(select * from OrderDetails where ProductID = @ProductId)
                                select 1
                            else 
	                            select 0";

                var parameters = new { ProductId = productId };

                result = connection.ExecuteScalar<bool>(sql: sql, param: parameters, commandType: CommandType.Text);

                connection.Close();
            }

            return result;
        }

        public IList<Product> List(int page = 1, int pageSize = 0, string searchValue = "", int categoryId = 0, int supplierId = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            List<Product> data = new List<Product>();

            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";

            using (var connection = OpenConnection())
            {
                var sql = @"with cte as(
                                select  *,
                                        row_number() over(order by ProductName) as RowNumber
                                from    Products
                                where   (@SearchValue = N'' or ProductName like @SearchValue)
                                    and (@CategoryID = 0 or CategoryID = @CategoryID)
                                    and (@SupplierID = 0 or SupplierID = @SupplierID)
                                    and (Price >= @MinPrice)
                                    and (@MaxPrice <= 0 or Price <= @MaxPrice)
                            )
                            select * from cte
                            where   (@PageSize = 0)
                                or (RowNumber between (@Page - 1)*@PageSize + 1 and @Page * @PageSize)";

                var parameters = new
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchValue = searchValue,
                    CategoryID = categoryId,
                    SupplierID = supplierId,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };

                data = connection.Query<Product>(sql: sql, param: parameters, commandType: CommandType.Text).ToList();

                connection.Close();
            }

            return data;
        }

        public IList<ProductAttribute> ListAttributes(int productId)
        {
            List<ProductAttribute> listAttributes = new List<ProductAttribute>();

            using (var connection = OpenConnection())
            {
                var sql = "select * from ProductAttributes where ProductID = @productID order by DisplayOrder asc";

                var parameters = new { productID = productId };

                listAttributes = connection.Query<ProductAttribute>(sql: sql, param: parameters, commandType: CommandType.Text).ToList();

                connection.Close();
            }

            return listAttributes;
        }

        public IList<ProductPhoto> ListPhotos(int productId)
        {
            List<ProductPhoto> listPhotos = new List<ProductPhoto>();

            using (var connection = OpenConnection())
            {
                var sql = "select * from ProductPhotos where ProductID = @productID order by DisplayOrder asc";

                var parameters = new { productID = productId };

                listPhotos = connection.Query<ProductPhoto>(sql: sql, param: parameters, commandType: CommandType.Text).ToList();

                connection.Close();
            }

            return listPhotos;
        }

        public bool Update(Product data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"if not exists(select * from Products where ProductID <> @ProductID and ProductName = @ProductName)
                                begin
                                    update Products
                                    set     ProductName = @productName,
                                            ProductDescription = @productDescription,
                                            SupplierID = @supplierId,
                                            CategoryID = @categoryId,
                                            Unit = @unit,
                                            Price = @price,
                                            Photo = @photo,
                                            IsSelling = @isSelling
                                    where ProductID = @productID
                                end";

                var parameters = new
                {
                    productId = data.ProductId,
                    productName = data.ProductName ?? "",
                    productDescription = data.ProductDescription ?? "",
                    supplierId = data.SupplierId,
                    categoryId = data.CategoryId,
                    unit = data.Unit,
                    price = data.Price,
                    photo = data.Photo ?? "",
                    isSelling = data.IsSelling,
                };

                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
            }

            return result;
        }

        public bool UpdateAttribute(ProductAttribute data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"if not exists(select * from ProductAttributes where AttributeID <> @AttributeId and AttributeName = @AttributeName and ProductID = @ProductId)
                                begin
                                    update ProductAttributes 
                                    set AttributeName = @AttributeName,
                                        AttributeValue = @AttributeValue,
                                        DisplayOrder = @DisplayOrder
                                    where AttributeID = @AttributeId
                                end";

                var parameters = new
                {
                    ProductId = data.ProductId,
                    AttributeId = data.AttributeId,
                    AttributeName = data.AttributeName ?? "",
                    AttributeValue = data.AttributeValue ?? "",
                    DisplayOrder = data.DisplayOrder,
                };

                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
            }

            return result;
        }

        public bool UpdatePhoto(ProductPhoto data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"update ProductPhotos
                            set Photo = @Photo,
                                Description = @Description,
                                DisplayOrder = @DisplayOrder,
                                IsHidden = @IsHidden
                            where PhotoID = @PhotoId";

                var parameters = new
                {
                    PhotoId = data.PhotoId,
                    ProductId = data.ProductId,
                    Photo = data.Photo ?? "",
                    Description = data.Description ?? "",
                    DisplayOrder = data.@DisplayOrder,
                    IsHidden = data.IsHidden
                };

                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
            }

            return result;
        }
    }
}
