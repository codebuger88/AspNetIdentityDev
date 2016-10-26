﻿using System;
using System.Collections.Generic;
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
        public async Task<ActionResult> Details(string id)
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
            var test = _db.BackendMenu;
            var qq = test.Include(s => s.BackendMenuAction).OrderBy(o => o.Sort);
            var qq2 = _db.BackendMenuAction.Include(s => s.BackendMenuPermission);
            ViewBag.Actions = test.Include(s => s.BackendMenuAction).OrderBy(o => o.Sort);
            ViewBag.Permission = _db.BackendMenuAction.Include(s => s.BackendMenuPermission);

            //foreach (var testsss in qq2)
            //{
            //    testsss.BackendMenuPermission
            //}

            var actions = _db.BackendMenuAction.Select(s => new BackendMenuActionViewModel()
            {
                ActionId = s.ActionId,
                Name = s.Name,
                /* Permissions = _db.BackendMenuPermission.Where(x => x.ActionId == s.ActionId)*/
                Permissions = s.BackendMenuPermission.ToList().Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.PermissionId.ToString()
                })
            }).AsEnumerable();

            model.Actions = actions;

            return View(model);
        }

        private IEnumerable<BackendMenuPermission> Permissions(IEnumerable<BackendMenuAction> model)
        {
            foreach (var action in model)
            {
                var data = _db.BackendMenuPermission.Where(x => x.ActionId == action.ActionId).AsEnumerable();

                foreach (var i in data)
                {
                    yield return i;
                }
            }
        }

        //
        // POST: /Users/Create
        [HttpPost]
        public async Task<ActionResult> Create(RegisterViewModel userViewModel, string[] permissions, params string[] selectedRoles)
        {
            var actions = _db.BackendMenuAction.Select(s => new BackendMenuActionViewModel()
            {
                ActionId = s.ActionId,
                Name = s.Name,
                /* Permissions = _db.BackendMenuPermission.Where(x => x.ActionId == s.ActionId)*/
                Permissions = s.BackendMenuPermission.ToList().Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.PermissionId.ToString()
                })
            }).AsEnumerable();



            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = userViewModel.Email, Email = userViewModel.Email, NickName = userViewModel.NickName };
                var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);

                //Add User to the selected Roles 
                if (adminresult.Succeeded)
                {
                    if (permissions != null)
                    {
                        foreach (var permission in permissions)
                        {
                            short permissionId = Convert.ToInt16(permission);
                            _db.BackendUserPermission.Add(new BackendUserPermission()
                            {
                                PermissionId = permissionId,
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

                            userViewModel.Actions = actions;

                            return View(userViewModel);

                            //return View();
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", adminresult.Errors.First());
                    ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");

                    userViewModel.Actions = actions;

                    return View(userViewModel);
                    //return View();

                }
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
            userViewModel.Actions = actions;

            return View(userViewModel);
            //return View();
        }

        //
        // GET: /Users/Edit/1
        public async Task<ActionResult> Edit(string id)
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
            var userPermissions = _db.BackendUserPermission.Where(x => x.UserId == user.Id).Select(s => s.PermissionId).ToList();
            var actions = _db.BackendMenuAction.Select(s => new BackendMenuActionViewModel()
            {
                ActionId = s.ActionId,
                Name = s.Name,
                Permissions = s.BackendMenuPermission.ToList().Select(x => new SelectListItem()
                {
                    Selected = userPermissions.Contains(x.PermissionId),
                    Text = x.Name,
                    Value = x.PermissionId.ToString()
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
                Actions = actions
            });
        }

        //
        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Email,Id")] EditUserViewModel editUser, string[] permissions, params string[] selectedRole)
        {
            var user = await UserManager.FindByIdAsync(editUser.Id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var userRoles = await UserManager.GetRolesAsync(user.Id);
            var userPermissions = _db.BackendUserPermission.Where(x => x.UserId == user.Id).Select(s => s.PermissionId).ToList();
            var actions = _db.BackendMenuAction.Select(s => new BackendMenuActionViewModel()
            {
                ActionId = s.ActionId,
                Name = s.Name,
                Permissions = s.BackendMenuPermission.ToList().Select(x => new SelectListItem()
                {
                    Selected = userPermissions.Contains(x.PermissionId),
                    Text = x.Name,
                    Value = x.PermissionId.ToString()
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
                    editUser.Actions = actions;
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

                    editUser.Actions = actions;
                    editUser.RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
                    {
                        Selected = userRoles.Contains(x.Name),
                        Text = x.Name,
                        Value = x.Name
                    });
                    return View(editUser);
                }

                foreach (var permission in permissions.Except(userPermissions.Select(s => s.ToString())))
                {
                    short permissionId = Convert.ToInt16(permission);
                    _db.BackendUserPermission.Add(new BackendUserPermission()
                    {
                        PermissionId = permissionId,
                        UserId = user.Id
                    });
                }

                foreach (var permission in userPermissions.Select(s => s.ToString()).Except(permissions))
                {
                    short permissionId = Convert.ToInt16(permission);
                    _db.BackendUserPermission.Remove(_db.BackendUserPermission.FirstOrDefault(x => x.PermissionId == permissionId && x.UserId == user.Id));
                }

                _db.SaveChanges();

                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Something failed.");
            editUser.Actions = actions;
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
        public async Task<ActionResult> Delete(string id)
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
        public async Task<ActionResult> DeleteConfirmed(string id)
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
