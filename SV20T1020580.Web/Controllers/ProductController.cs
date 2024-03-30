using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020580.BusinessLayers;
using SV20T1020580.DomainModels;
using SV20T1020580.Web.Models;

namespace SV20T1020580.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator},{WebUserRoles.Employee}")]
    public class ProductController : Controller
    {
        const int PAGE_SIZE = 20;
        const string PRODUCT_SEARCH = "product_search";

        public IActionResult Index()
        {
            //Kiểm tra xem trong session có lưu điều kiện tìm kiếm không
            //Nếu có thì sử dụng lại điều kiện tìm kiếm, ngược lại thì tìm kiếm theo điều kiện mặc định
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH);

            if (input == null)
            {
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = "",
                    CategoryId = 0,
                    SupplierId = 0,
                };
            }

            return View(input);
        }

        public IActionResult Search(ProductSearchInput input)
        {
            int rowCount = 0;
            var data = ProductDataService.ListProducts(out rowCount, input.Page, input.PageSize, input.SearchValue ?? "", input.CategoryId,
                                            input.SupplierId, input.MinPrice, input.MaxPrice);

            var model = new ProductSearchResult()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                RowCount = rowCount,
                Data = data
            };

            // Lưu lại vào session điều kiện tìm kiếm
            ApplicationContext.SetSessionData(PRODUCT_SEARCH, input);

            return View(model);
        }


        //public IActionResult Index(int page = 1, string searchValue = "", int categoryId = 0, int supplierId = 0, decimal minPrice = 0, decimal maxPrice = 0)
        //{
        //    int rowCount = 0;

        //    var data = ProductDataService.ListProducts(out rowCount, page, PAGE_SIZE, searchValue ?? "", categoryId, supplierId, minPrice, maxPrice);

        //    var model = new ProductSearchResult()
        //    {
        //        Page = page,
        //        PageSize = PAGE_SIZE,
        //        SearchValue = searchValue ?? "",
        //        CategoryId = categoryId,
        //        SupplierId = supplierId,
        //        RowCount = rowCount,
        //        Data = data
        //    };

        //    return View(model);
        //}

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung mặt hàng";
            ViewBag.IsEdit = false;
            var model = new Product()
            {
                ProductId = 0,
                Photo = "nophoto.png",
            };

            return View("Edit", model);
        }

        public IActionResult Edit(int Id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin mặt hàng";
            ViewBag.IsEdit = true;

            var model = ProductDataService.GetProduct(Id);
            if (model == null)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Save(Product model, IFormFile? uploadPhoto = null)
        {
            if (string.IsNullOrWhiteSpace(model.ProductName))
                ModelState.AddModelError(nameof(model.ProductName), "Tên mặt hàng không được để trống");
            if (string.IsNullOrWhiteSpace(model.ProductDescription))
                ModelState.AddModelError(nameof(model.ProductDescription), "Mô tả không được để trống");
            if (model.CategoryId.ToString() == "0")
                ModelState.AddModelError(nameof(model.CategoryId), "Vui lòng chọn Loại hàng");
            if (model.SupplierId.ToString() == "0")
                ModelState.AddModelError(nameof(model.SupplierId), "Vui lòng chọn Nhà cung cấp");
            if (string.IsNullOrWhiteSpace(model.Unit))
                ModelState.AddModelError(nameof(model.Unit), "Đơn vị tính không được để trống");
            if (!decimal.TryParse(model.Price.ToString(), out decimal parsedPrice) || parsedPrice <= 0)
                ModelState.AddModelError("Price", "Giá hàng không hợp lệ.");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.ProductId == 0 ? "Bổ sung mặt hàng" : "Cập nhật thông tin mặt hàng";
                ViewBag.IsEdit = model.ProductId == 0 ? false : true;

                return View("Edit", model);
            }

            // Xử lý ảnh upload: Nếu có ảnh được upload thì lưu ảnh lên server, gán tên file ảnh đã lưu cho model.Photo
            if (uploadPhoto != null)
            {
                // Tên file sẽ lưu trên server 
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";

                // Đường dẫn vật lý đến file sẽ lưu trên server (vd: D:\MyWeb\wwwroot\images\employees\photo.png)
                string filePath = Path.Combine(ApplicationContext.HostEnviroment.WebRootPath, @"images\products", fileName);

                // Lưu file lên server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadPhoto.CopyTo(stream);
                }

                // Gán tên file ảnh cho model.Photo
                model.Photo = fileName;
            }

            if (model.ProductId == 0)
            {
                int id = ProductDataService.AddProduct(model);
                if (id <= 0)
                {
                    ModelState.AddModelError("ProductName", "Tên mặt hàng bị trùng");
                    ViewBag.Title = "Bổ sung mặt hàng";
                    return View("Edit", model);
                }
            }
            else
            {
                bool result = ProductDataService.UpdateProduct(model);
                if (!result)
                {
                    ModelState.AddModelError("Error", "Không cập nhật được mặt hàng. Có thể tên mặt hàng bị trùng");
                    ViewBag.Title = "Cập nhật thông tin mặt hàng";
                    return View("Edit", model);
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int Id = 0)
        {
            if (Request.Method == "POST")
            {
                bool result = ProductDataService.DeleteProduct(Id);
                return RedirectToAction("Index");
            }

            var model = ProductDataService.GetProduct(Id);
            if (model == null)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public IActionResult Photo(int id = 0, string method = "add", long photoId = 0)
        {
            if (id < 0)
            {
                return RedirectToAction("Index");
            }

            ProductPhoto data = null;
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung ảnh cho mặt hàng";

                    data = new ProductPhoto()
                    {
                        PhotoId = 0,
                        ProductId = id,
                    };

                    return View(data);
                case "edit":
                    ViewBag.Title = "Cập nhật ảnh của mặt hàng";

                    if (photoId < 0)
                    {
                        return RedirectToAction("Index");
                    }

                    data = ProductDataService.GetPhoto(photoId);
                    if (data == null)
                    {
                        return RedirectToAction("Index");
                    }

                    return View(data);
                case "delete":
                    //TODO: Xóa ảnh có mã photoId (xóa trực tiếp, không cần xác nhận)
                    ProductDataService.DeletePhoto(photoId);
                    return RedirectToAction("Edit", new { id = id });
                default:
                    return RedirectToAction("Index");
            }
        }

        public IActionResult AddPhoto(ProductPhoto model, IFormFile? uploadPhoto)
        {
            if (uploadPhoto == null)
                ModelState.AddModelError(nameof(model.Photo), "Vui lòng chọn ảnh");
            if (string.IsNullOrWhiteSpace(model.Description))
                ModelState.AddModelError(nameof(model.Description), "Mô tả không được để trống");
            if (!int.TryParse(model.DisplayOrder.ToString(), out int parsedDisplayOrder) || parsedDisplayOrder <= 0)
                ModelState.AddModelError("DisplayOrder", "Thứ tự hiển thị ảnh mặt hàng phải > 0");

            // Kiểm tra thứ tự hiển thị
            List<ProductPhoto> lstPhotos = ProductDataService.ListPhotos(model.ProductId);
            bool isUsed = false;

            foreach (ProductPhoto item in lstPhotos)
            {
                if (item.DisplayOrder == model.DisplayOrder && model.PhotoId != item.PhotoId)
                {
                    isUsed = true;
                    break;
                }
            }

            if (isUsed)
            {
                ModelState.AddModelError("DisplayOrder",
                        $"Thứ tự hiển thị {model.DisplayOrder} của ảnh đã được sử dụng");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.PhotoId == 0 ? "Bổ sung ảnh cho mặt hàng" : "Cập nhật ảnh của mặt hàng";
                return View("Photo", model);
            }

            // Xử lý ảnh upload: Nếu có ảnh được upload thì lưu ảnh lên server, gán tên file ảnh đã lưu cho model.Photo
            if (uploadPhoto != null)
            {
                // Tên file sẽ lưu trên server 
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";

                // Đường dẫn vật lý đến file sẽ lưu trên server (vd: D:\MyWeb\wwwroot\images\employees\photo.png)
                string filePath = Path.Combine(ApplicationContext.HostEnviroment.WebRootPath, @"images\products", fileName);

                // Lưu file lên server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadPhoto.CopyTo(stream);
                }

                // Gán tên file ảnh cho model.Photo
                model.Photo = fileName;
            }

            // Thêm hoặc Cập nhật
            if (model.PhotoId == 0)
            {
                long ptId = ProductDataService.AddPhoto(model);
                if (ptId <= 0)
                {
                    ModelState.AddModelError("Error", "Không thêm được ảnh cho mặt hàng");
                    ViewBag.Title = "Bổ sung ảnh cho mặt hàng";
                    return View("Photo", model);
                }
            }
            else
            {
                bool result = ProductDataService.UpdatePhoto(model);
                if (!result)
                {
                    ModelState.AddModelError("Error", "Không cập nhật được ảnh cho mặt hàng");
                    ViewBag.Title = "Cập nhật ảnh của mặt hàng";
                    return View("Photo", model);
                }
            }

            return RedirectToAction("Edit", new { Id = model.ProductId });
        }

        public IActionResult Attribute(int id = 0, string method = "add", int attributeId = 0)
        {
            if (id < 0)
            {
                return RedirectToAction("Index");
            }

            ProductAttribute data = null;
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung thuộc tính cho mặt hàng";

                    data = new ProductAttribute()
                    {
                        AttributeId = 0,
                        ProductId = id,
                    };

                    return View(data);
                case "edit":
                    ViewBag.Title = "Cập nhật thuộc tính của mặt hàng";
                    if (attributeId < 0)
                    {
                        return RedirectToAction("Index");
                    }

                    data = ProductDataService.GetAttribute(attributeId);
                    if (data == null)
                    {
                        return RedirectToAction("Index");
                    }

                    return View(data);
                case "delete":
                    ProductDataService.DeleteAttribute(attributeId);
                    return RedirectToAction("Edit", new { id = id });
                default:
                    return RedirectToAction("Index");
            }
        }

        public IActionResult AddAttribute(ProductAttribute model)
        {
            if (string.IsNullOrWhiteSpace(model.AttributeName))
                ModelState.AddModelError(nameof(model.AttributeName), "Tên thuộc tính không được rỗng");
            if (string.IsNullOrWhiteSpace(model.AttributeValue))
                ModelState.AddModelError(nameof(model.AttributeValue), "Giá trị thuộc tính không được rỗng");
            if (!int.TryParse(model.DisplayOrder.ToString(), out int parsedDisplayOrder) || parsedDisplayOrder <= 0)
                ModelState.AddModelError("DisplayOrder", "Thứ tự hiển thị thuộc tính phải > 0");

            // Kiểm tra thứ tự hiển thị
            List<ProductAttribute> lstAttributes = ProductDataService.ListAttributes(model.ProductId);
            bool isUsed = false;

            foreach (ProductAttribute item in lstAttributes)
            {
                if (item.DisplayOrder == model.DisplayOrder && model.AttributeId != item.AttributeId)
                {
                    isUsed = true;
                    break;
                }
            }

            if (isUsed)
            {
                ModelState.AddModelError("DisplayOrder",
                        $"Thứ tự hiển thị {model.DisplayOrder} của thuộc tính đã được sử dụng");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.AttributeId == 0 ? "Bổ sung thuộc tính cho mặt hàng" : "Cập nhật thuộc tính của mặt hàng";
                return View("Attribute", model);
            }


            // Thêm hoặc Cập nhật
            if (model.AttributeId == 0)
            {
                long atId = ProductDataService.AddAttribute(model);
                if (atId <= 0)
                {
                    ModelState.AddModelError(nameof(model.AttributeName), "Tên thuộc tính bị trùng");
                    ViewBag.Title = "Bổ sung thuộc tính cho mặt hàng";
                    return View("Attribute", model);
                }
            }
            else
            {
                bool result = ProductDataService.UpdateAttribute(model);
                if (!result)
                {
                    ModelState.AddModelError("Error", "Không cập nhật được thuộc tính. Có thể tên thuộc tính bị trùng");
                    ViewBag.Title = "Cập nhật thuộc tính của mặt hàng";
                    return View("Attribute", model);
                }
            }


            return RedirectToAction("Edit", new { id = model.ProductId });
        }
    }
}
