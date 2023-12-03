using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Users.Models
{
    public class RoleAdminController : Controller
    {
        private RoleManager<IdentityRole> roleManager;
        private UserManager<AppUser> userManager;
        public RoleAdminController(RoleManager<IdentityRole> role, UserManager<AppUser> roleMng)
        {
            roleManager = role;
            userManager = roleMng;
        }
        public ViewResult Index() => View(roleManager.Roles);
        public IActionResult Create() => View();
        [HttpPost]
        public async Task<IActionResult> Create([Required]string name)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await roleManager.CreateAsync(new IdentityRole(name));
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            return View(name);
        }
        
        [HttpPost]
        public async Task   <IActionResult> Delete(string id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);
            if(role!=null)
            {
                IdentityResult identityResult = await roleManager.DeleteAsync(role);
                if (identityResult.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsFromResult(identityResult);
                }
            }
            else
            {
                ModelState.AddModelError(" ", "Not Role Found!");
            }
            return View("Index", roleManager.Roles);
        }
        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach(IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        public async Task<IActionResult> Edit(string id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);
            List<AppUser> members = new List<AppUser>();
            List<AppUser> nonMembers = new List<AppUser>();
            foreach (AppUser user in userManager.Users)
            {
                var list = await userManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
                list.Add(user);
            }
            return View(new RoleEditModel
            {
                Role = role,
                Members = members,
                NonMembers = nonMembers

            });

        }
        [HttpPost]
        public async Task<IActionResult> Edit(RoleModificationModel model)
        {
            IdentityResult result;
            if(ModelState.IsValid)
            {
                foreach(string userId in model.IdsToAdd?? new string[] { })
                {
                    AppUser user = await userManager.FindByIdAsync(userId);
                    if(user!=null)
                    {
                        result = await userManager.AddToRoleAsync(user, model.RoleName);
                        if(!result.Succeeded)
                        {
                            AddErrorsFromResult(result);
                        }
                    }
                }
                foreach (string userId in model.IdsToDelete ?? new string[] { })
                {
                    AppUser user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await userManager.RemoveFromRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                        {
                            AddErrorsFromResult(result);
                        }
                    }
                }
            }
            if(ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));

            }
            else
            {
                return await Edit(model.Roleid);
            }
            
        }
    }
}