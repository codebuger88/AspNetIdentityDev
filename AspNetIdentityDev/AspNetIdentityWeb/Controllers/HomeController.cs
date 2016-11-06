using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using AspNetIdentityWeb.Caching;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace AspNetIdentityWeb.Controllers
{
    public class HomeController : Controller
    {
        private ICacheProvider Cache => new CacheProvider();

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set { _userManager = value; }
        }

        public ActionResult Index(int? id)
        {
            int userId = id ?? 2;

            IList<string> userRoles = new List<string>();
            if (Cache.Get("CurrentRoles") == null)
            {
                userRoles = UserManager.GetRoles(userId);
                Cache.Set("CurrentRoles", userRoles, 60);
            }
            userRoles = Cache.Get("CurrentRoles") as List<string>;

            foreach (var userRole in userRoles)
            {
                Response.Write(userRole);
                Response.Write("<br />");
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}