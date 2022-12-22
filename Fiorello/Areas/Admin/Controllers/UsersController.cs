using Fiorello.Models;
using Fiorello.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fiorello.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UsersController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        #region Index
        public async Task<IActionResult> Index()
        {
            List<AppUser> users = await _userManager.Users.ToListAsync();
            List<UserVM> userVMs = new List<UserVM>();
            foreach (AppUser user in users)
            {
                UserVM userVM = new UserVM
                {
                    Id = user.Id,
                    Name = user.Name,
                    Surname = user.Surname,
                    Username = user.UserName,
                    Email = user.Email,
                    IsDeactive = user.IsDeactive,
                    Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
                };
                userVMs.Add(userVM);
            }
            return View(userVMs);
        }
        #endregion

        #region Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterVM registerVM,string role)
        {
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            if (!ModelState.IsValid)
            {
                return View();
            }
            AppUser appUser = new AppUser
            {
                Name = registerVM.Name,
                Surname = registerVM.Surname,
                UserName = registerVM.Username,
                Email = registerVM.Email
            };
            IdentityResult identityResult = await _userManager.CreateAsync(appUser, registerVM.Password);
            if (!identityResult.Succeeded)
            {
                foreach (IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }
            await _userManager.AddToRoleAsync(appUser, role);
            return RedirectToAction("Index");
        }
        #endregion

        #region Update
        public async Task<IActionResult> Update(string id)
        {
            if (id==null)
            {
                return NotFound();
            }
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest();
            }
            UpdateVM updateVM = new UpdateVM()
            {
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Username = user.UserName,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
            };
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            return View(updateVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateVM updateVM, string newRole, string id)
        {
            #region From Get
            if (id == null)
            {
                return NotFound();
            }
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest();
            }
            UpdateVM dbupdateVM = new UpdateVM()
            {
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Username = user.UserName,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
            };
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            #endregion
            if (!ModelState.IsValid)
            {
                return View(dbupdateVM);
            }
            user.Name = updateVM.Name;
            user.Surname = updateVM.Surname;
            user.Email = updateVM.Email;
            user.UserName = updateVM.Username;
            if (newRole!=dbupdateVM.Role)
            {
                IdentityResult addidentityResult = await _userManager.AddToRoleAsync(user, newRole);
                if (!addidentityResult.Succeeded)
                {
                    foreach (IdentityError error in addidentityResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                        return View();
                    }
                }
                IdentityResult removeidentityResult = await _userManager.RemoveFromRoleAsync(user, dbupdateVM.Role);
                if (!removeidentityResult.Succeeded)
                {
                    foreach (IdentityError error in removeidentityResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                        return View();
                    }
                }
            }
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }
        #endregion

        #region ResetPassword
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest();
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM resetPasswordVM, string id)
        {
            #region From Get
            if (id == null)
            {
                return NotFound();
            }
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest();
            }
            #endregion
            if (!ModelState.IsValid)
            {
                return View();
            }

            string token =await _userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult identityResult= await _userManager.ResetPasswordAsync(user,token,resetPasswordVM.Password);
            if (!identityResult.Succeeded)
            {
                foreach (IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return RedirectToAction("Index");
        }
        #endregion
    }
}
