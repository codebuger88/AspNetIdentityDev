using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AspNetIdentityWeb.Models;
using AspNetIdentityWeb.Models.ViewModels;
using Microsoft.AspNet.Identity.Owin;

namespace AspNetIdentityWeb.Controllers
{
    public class UserAdminController : Controller
    {
        private WebEntities _db = new WebEntities();

        public UserAdminController()
        {

        }

        public UserAdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set { _userManager = value; }
        }

        private ApplicationRoleManager _roleManager;

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set { _roleManager = value; }
        }

        // GET: UserAdmin
        public async Task<ActionResult> Index()
        {
            return View(await UserManager.Users.ToListAsync());
        }

        //
        // GET: /Users/Details/5
        public async Task<ActionResult> Details(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);

            ViewBag.RoleNames = await UserManager.GetRolesAsync(user.Id);

            return View(user);
        }

        //
        // GET: /Users/Create
        public async Task<ActionResult> Create(RegisterViewModel model)
        {
            //Get the list of Roles
            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");

            var actions = _db.BackendMenu.Select(s => new BackendMenuViewModel()
            {
                MenuId = s.MenuId,
                Name = s.Name,
                /* Permissions = _db.BackendMenuAction.Where(x => x.ActionId == s.ActionId)*/
                Actions = s.BackendMenuAction.ToList().Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.ActionId.ToString()
                })
            }).AsEnumerable();

            model.Menus = actions;

            return View(model);
        }

        //
        // POST: /Users/Create
        [HttpPost]
        public async Task<ActionResult> Create(RegisterViewModel userViewModel, string[] actions, params string[] selectedRoles)
        {
            var menus = _db.BackendMenu.Select(s => new BackendMenuViewModel()
            {
                MenuId = s.MenuId,
                Name = s.Name,
                /* Permissions = _db.BackendMenuAction.Where(x => x.MenuId == s.MenuId)*/
                Actions = s.BackendMenuAction.ToList().Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.ActionId.ToString()
                })
            }).AsEnumerable();



            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = userViewModel.Email, Email = userViewModel.Email, NickName = userViewModel.NickName };
                var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);

                //Add User to the selected Roles 
                if (adminresult.Succeeded)
                {
                    if (actions != null)
                    {
                        foreach (var action in actions)
                        {
                            short actionId = Convert.ToInt16(action);
                            _db.BackendUserPermission.Add(new BackendUserPermission()
                            {
                                ActionId = actionId,
                                UserId = user.Id
                            });
                        }

                        _db.SaveChanges();
                    }

                    if (selectedRoles != null)
                    {
                        var result = await UserManager.AddToRolesAsync(user.Id, selectedRoles);
                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError("", result.Errors.First());
                            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");

                            userViewModel.Menus = menus;

                            return View(userViewModel);

                            //return View();
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", adminresult.Errors.First());
                    ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");

                    userViewModel.Menus = menus;

                    return View(userViewModel);
                    //return View();

                }
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
            userViewModel.Menus = menus;

            return View(userViewModel);
            //return View();
        }

        //
        // GET: /Users/Edit/1
        public async Task<ActionResult> Edit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userRoles = await UserManager.GetRolesAsync(user.Id);
            var userPermissions = _db.BackendUserPermission.Where(x => x.UserId == user.Id).Select(s => s.ActionId).ToList();
            var menus = _db.BackendMenu.Select(s => new BackendMenuViewModel()
            {
                MenuId = s.MenuId,
                Name = s.Name,
                Actions = s.BackendMenuAction.ToList().Select(x => new SelectListItem()
                {
                    Selected = userPermissions.Contains(x.ActionId),
                    Text = x.Name,
                    Value = x.ActionId.ToString()
                })
            }).AsEnumerable();

            return View(new EditUserViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
                {
                    Selected = userRoles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                }),
                Menus = menus
            });
        }

        //
        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Email,Id")] EditUserViewModel editUser, string[] actions, params string[] selectedRole)
        {
            var user = await UserManager.FindByIdAsync(editUser.Id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var userRoles = await UserManager.GetRolesAsync(user.Id);
            var userPermissions = _db.BackendUserPermission.Where(x => x.UserId == user.Id).Select(s => s.ActionId).ToList();
            var menus = _db.BackendMenu.Select(s => new BackendMenuViewModel()
            {
                MenuId = s.MenuId,
                Name = s.Name,
                Actions = s.BackendMenuAction.ToList().Select(x => new SelectListItem()
                {
                    Selected = userPermissions.Contains(x.ActionId),
                    Text = x.Name,
                    Value = x.ActionId.ToString()
                })
            }).AsEnumerable();

            if (ModelState.IsValid)
            {
                //var user = await UserManager.FindByIdAsync(editUser.Id);
                //if (user == null)
                //{
                //    return HttpNotFound();
                //}

                user.UserName = editUser.Email;
                user.Email = editUser.Email;

                //var userRoles = await UserManager.GetRolesAsync(user.Id);

                selectedRole = selectedRole ?? new string[] { };

                var result = await UserManager.AddToRolesAsync(user.Id, selectedRole.Except(userRoles).ToArray<string>());


                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    editUser.Menus = menus;
                    editUser.RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
                    {
                        Selected = userRoles.Contains(x.Name),
                        Text = x.Name,
                        Value = x.Name
                    });

                    return View(editUser);
                }
                result = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.Except(selectedRole).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());

                    editUser.Menus = menus;
                    editUser.RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
                    {
                        Selected = userRoles.Contains(x.Name),
                        Text = x.Name,
                        Value = x.Name
                    });
                    return View(editUser);
                }

                if (actions != null)
                {
                    foreach (var permission in actions.Except(userPermissions.Select(s => s.ToString())))
                    {
                        short ActionId = Convert.ToInt16(permission);
                        _db.BackendUserPermission.Add(new BackendUserPermission()
                        {
                            ActionId = ActionId,
                            UserId = user.Id
                        });
                    }

                    foreach (var permission in userPermissions.Select(s => s.ToString()).Except(actions))
                    {
                        short ActionId = Convert.ToInt16(permission);
                        _db.BackendUserPermission.Remove(_db.BackendUserPermission.FirstOrDefault(x => x.ActionId == ActionId && x.UserId == user.Id));
                    }

                    _db.SaveChanges();
                }

                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Something failed.");
            editUser.Menus = menus;
            editUser.RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
            {
                Selected = userRoles.Contains(x.Name),
                Text = x.Name,
                Value = x.Name
            });
            return View(editUser);
        }

        //
        // GET: /Users/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                var result = await UserManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
