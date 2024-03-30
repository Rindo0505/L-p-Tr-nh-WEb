using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020580.BusinessLayers;

namespace SV20T1020580.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
           
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Login(string username="", string password = "")
        {
            ViewBag.Username = username;
            //Kiểm tra xem thông tin nhập có đủ không
            if(string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Nhập đủ tên và mật khẩu");
                return View();
            }
            //Kiểm tra thông tin đăng nhập có hợp lệ hay không
            var userAccount = UserAccountService.Authorize(username, password);
            if(userAccount == null)
            {
                ModelState.AddModelError("Error", "Đăng nhập thất bại");
                return View();  
            }

            //Đăng nhập thành công, tạo dữ liệu để lưu cookie
            WebUserData userData = new WebUserData()
            {
                UserId = userAccount.UserID,
                UserName = userAccount.UserName,
                DisplayName = userAccount.FullName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                ClientIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                SessionId = HttpContext.Session.Id,
                AdditionalData ="",
                Roles = userAccount.RoleNames.Split(',').ToList(),
            };
            //Thiết lập phiên đăng nhập cho tài khoản
            await HttpContext.SignInAsync(userData.CreatePrincipal());


            return RedirectToAction("Index", "Home");
        }

        public async  Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmNewPassword)
        {
            bool result = false;

            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword)
                        || string.IsNullOrWhiteSpace(confirmNewPassword))
            {
                ModelState.AddModelError("Error", "Bạn cần điền đầy đủ thông tin");
                return View();
            }

            string username = User?.GetUserData().UserName ?? "";

            var userAccount = UserAccountService.Authorize(username, oldPassword);

            if (userAccount != null)
            {
                if (newPassword != confirmNewPassword)
                {
                    ModelState.AddModelError("Error", "Mật khẩu mới và xác nhận mật khẩu không giống nhau");
                    return View();
                }

                if (oldPassword == newPassword)
                {
                    ModelState.AddModelError("Error", "Mật khẩu mới và mật khẩu cũ không được trùng nhau");
                    return View();
                }
            }
            else
            {
                ModelState.AddModelError("Error", "Mật khẩu cũ chưa chính xác");
                return View();
            }

            result = UserAccountService.ChangePassword(username, oldPassword, newPassword);
            if (result)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("Error", "Đổi mật khẩu không thành công");
                return View();
            }

            // return View();
        }
        public IActionResult AccessDenined()
        {
            return View();
        }
    }
}
