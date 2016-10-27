using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace AspNetIdentityWeb.Models
{
    // 您可以在 ApplicationUser 類別新增更多屬性，為使用者新增設定檔資料，請造訪 http://go.microsoft.com/fwlink/?LinkID=317594 以深入了解。
    public class ApplicationUser : IdentityUser<int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>
    {
        public string NickName { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager)
        {
            // 注意 authenticationType 必須符合 CookieAuthenticationOptions.AuthenticationType 中定義的項目
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // 在這裡新增自訂使用者宣告
            return userIdentity;
        }
    }

    public class ApplicationUserRole : IdentityUserRole<int>
    {
    }

    public class ApplicationUserLogin : IdentityUserLogin<int>
    {
    }

    public class ApplicationUserClaim : IdentityUserClaim<int>
    {
    }

    public class ApplicationRole : IdentityRole<int, ApplicationUserRole>
    {
        public ApplicationRole() { }
        public ApplicationRole(string name) { Name = name; }
    }

    public class ApplicationRoleStore :
        RoleStore<ApplicationRole, int, ApplicationUserRole>
    {
        public ApplicationRoleStore(ApplicationDbContext context)
            : base(context)
        {
        }
    }

    public class ApplicationUserStore :
        UserStore<ApplicationUser, ApplicationRole, int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>
    {
        public ApplicationUserStore(ApplicationDbContext context)
            : base(context)
        {
        }
    }

    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, ApplicationRole, int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>
    {
        public ApplicationDbContext()
            : base("IdentityDbContext")
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}