using System.Web.Mvc;

namespace i18n.Demo.MVC4.Controllers
{
    public class HomeController : LocalizingController
    {
        public ActionResult Index()
        {
            ViewBag.Message = __("i18n string from controller"); //"Modify this template to jump-start your ASP.NET MVC application.";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
