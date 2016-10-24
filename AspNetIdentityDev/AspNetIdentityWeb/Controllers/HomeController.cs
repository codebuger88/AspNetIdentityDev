using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AspNetIdentityWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace AspNetIdentityWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
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

        public List<ApplicationUser> GetUsersInRole(string roleName)
        {
            var db = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var role = roleManager.FindByName(roleName).Users.First();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            //ApplicationUserManager userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            string userId = User.Identity.GetUserId();
            var roles = userManager.GetRoles(userId);
            if (roles.Contains("SysAdmin"))
            {
            }

            //userManager.FindByIdAsync()

            var usersInRole = userManager.Users.Where(u => u.Roles.Select(r => r.RoleId).Contains(role.RoleId)).ToList();
            return usersInRole;
        }
    }
}