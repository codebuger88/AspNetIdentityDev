using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using AspNetIdentityWeb.Caching;
using AspNetIdentityWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace AspNetIdentityWeb.Helpers
{
    public class PermissionHelper
    {
        private static ICacheProvider Cache => new CacheProvider();

        private static ApplicationUserManager _userManager;
        public static ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set { _userManager = value; }
        }

        public static bool CheckUserHasPermision(int userId, string permissionName)
        {
            int minute = 60;
            WebEntities context;
            List<AspNetRoles> list = new List<AspNetRoles>();
            List<BackendUserPermission> list2 = new List<BackendUserPermission>();
            if (Cache.Get("Roles") == null)
            {
                using (context = new WebEntities())
                {
                    list = context.AspNetRoles.AsEnumerable<AspNetRoles>().ToList<AspNetRoles>();
                    Cache.Set("Roles", list, minute);
                }
            }
            if (Cache.Get("BackendUserPermission") == null)
            {
                using (context = new WebEntities())
                {
                    list2 = context.BackendUserPermission.Include(s => s.BackendMenuAction).AsEnumerable<BackendUserPermission>().ToList<BackendUserPermission>();
                    Cache.Set("BackendUserPermission", list2, minute);
                }
            }
            IList<string> userRoles = new List<string>();
            if (Cache.Get("CurrentRoles") == null)
            {
                userRoles = UserManager.GetRoles(userId);
                Cache.Set("CurrentRoles", userRoles, minute);
            }
            userRoles = Cache.Get("CurrentRoles") as List<string>;
            list = Cache.Get("Roles") as List<AspNetRoles>;
            list2 = Cache.Get("BackendUserPermission") as List<BackendUserPermission>;
            IList<string> strArray2 = userRoles;
            for (int i = 0; i < strArray2.Count; i++)
            {
                Func<BackendUserPermission, bool> predicate = null;
                string roleName = strArray2[i];
                
                List<BackendMenuAction> list3 = (from e in list2 select e.BackendMenuAction).ToList<BackendMenuAction>();
                foreach (BackendMenuAction permission in list3)
                {
                    if (permission.Name == permissionName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}