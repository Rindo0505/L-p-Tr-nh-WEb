using Microsoft.AspNetCore.Mvc;

namespace SV20T1020580.Web.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Create()
        {
            var model = new Models.Person()
            {
                Name = "Trương Bá Thưởng",
                Birthday = new DateTime(1990,10,28),
                Salary = 500.25m
            };
            return View(model);
        }
        public IActionResult Save(Models.Person model)
        {
            return Json(model);
        }
    }
}
